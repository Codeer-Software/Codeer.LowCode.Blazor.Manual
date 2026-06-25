# 可視化・ダッシュボードのパターン（ガント / タスクボード / グラフ）

ガントチャート（`GanttField`）・タスクボード（`TaskBoardField`）・グラフ（`ApexChartField` 系）を業務アプリに組み込むときの**実用パターン**。各フィールドのプロパティ一覧は [Fields/GanttField.md](../Fields/GanttField.md) / [Fields/TaskBoardField.md](../Fields/TaskBoardField.md) / [Fields/ApexChartField.md](../Fields/ApexChartField.md) を参照。ここは「**どう結線して動く画面にするか**」をまとめる。

> いずれも外部ライブラリのフィールド。`app.clprj` の `Versions` に `Codeer.LowCode.Blazor.Extras`（ガント/ボード）・`Codeer.LowCode.Bindings.ApexCharts`（グラフ）を入れる。ガントとグラフはヘッドレスでは描画されないことがある（実機で確認）。

> **実装サンプル（必読）**: [`Samples/ProjectManagementTemplate/`](../../Samples/ProjectManagementTemplate/App/Modules/) に実物がある。`プロジェクト.mod.json`＝ガント＋タスクボードを親詳細に埋め込んだ例（編集レイアウト・`DataOnlyFields`・`IsFillAvailable` 行まで）、`Home.mod.json`＝チャート群、`クエリ/` 配下＝集計クエリモジュール＋ GROUP BY SQL。本ページの結線はこのサンプルに基づく。複製して差分だけ直すのが速い。

---

## 1. ガント / タスクボードを「親レコードの詳細」に埋め込む（ヘッダ＋明細と同じ構図）

これが最も確実な使い方。**ヘッダ＋明細（`ListField`）と全く同じ親子構造**で、明細を「表」でなく「ガントのバー」「ボードのカード」として見せる版だと考える。

### 仕組み（重要）

- ガント/タスクボードは**値を持たない表示系フィールド**（DB列なし）。対象モジュール（タスク表）の子レコード群を内部に保持し、**親モジュールの `Submit` で一括保存**される（`ListField` と同じ）。
- 子の絞り込みは `SearchCondition.Condition` の **`FieldVariableMatchCondition`**：`SearchTargetVariable`＝子のFKフィールド、`Variable`＝親の `Id.Value`。
- **親FKは追加時に自動セットされる**（`ListField` と同等）。バー/カードを追加すると、上記 `FieldVariableMatchCondition`（`Equal` / `GreaterThanOrEqual`）が親の値で解決され子のFKへ代入される。子モジュール側に FK フィールド（`IdField` か `LinkField`）を必ず持たせること。
- 子（タスク）モジュールには **`DataSourceName` が必須**（無いと読み込みが空になる）。

### 最小例（製造オーダー詳細に工程タスクをガント＋ボードで表示）

```json
// ManufacturingOrder.mod.json の Fields に（子=ProductionTask、FK=manufacturing_order_id）
{
  "DisplayName": "ガント",
  "SearchCondition": {
    "SortConditions": [ { "Variable": "計画開始日.Value", "IsDescending": false } ],
    "ModuleName": "ProductionTask",
    "Condition": {
      "IsOrMatch": false, "IsNot": false,
      "Children": [
        { "SearchTargetVariable": "ManufacturingOrderId.Value", "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition" }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "TextField": "工程名", "StartField": "計画開始日", "EndField": "計画終了日",
  "ProgressField": "進捗", "IdField": "Id", "ProcessingCounterField": "並び順",
  "DetailLayoutName": "ガント編集",
  "EnableDayView": false, "EnableWeekView": true, "EnableMonthView": true, "FitToWidth": true,
  "Name": "Gantt",
  "TypeFullName": "Codeer.LowCode.Blazor.Extras.Designs.GanttFieldDesign"
}
```

タスクボードも同じ `SearchCondition`（同じ FK 絞り込み）で、`StatusField` / `SortIndexField` / `PopupLayoutName` / `Statuses` を足す（次節）。

### 編集ダイアログ用レイアウト（DetailLayoutName / PopupLayoutName）の作法

バー/カードをクリックすると、対象モジュールの **DetailLayout（`DetailLayoutName` / `PopupLayoutName` で指定）がダイアログ**で開く。このレイアウトには:

- **`SubmitButton` を置かない**。保存はダイアログの OK で確定 → フィールドが内部保持 → 親モジュールの `Submit` でまとめて永続化される。
- **親FK と 並び順（SortIndex）を `DataOnlyFields` に入れる**（画面に出さずデータとして保持）。例: `"DataOnlyFields": ["並び順", "ManufacturingOrderId"]`。
- 工程名・担当者・日付・進捗・ステータス等、ユーザーが編集する項目を `Rows` に置く。

---

## 2. タスクボードのステータス列（Statuses）

- `Statuses.Items` は列定義の**配列**。各要素は `DisplayText`（列見出し）/ `Value`（カードの所属を決める実値）/ `Color`（文字色）/ `BackgroundColor`（列背景）/ `CanAdd`（＋追加ボタン、既定 `true`）。**「表示,値」の区切り文字方式ではない**（SelectField の `Candidates` とは書式が違う）。
- カードの所属列は `StatusField` の文字列値と `Statuses.Items[i].Value` の**文字列一致**で決まる。
- **`StatusField` は `SelectField` か `TextField` のみ**（RadioGroup 等は不可）。
- `StatusField` に `SelectField` を使うなら、**`Candidates` の「値」部と `Statuses.Items[i].Value` を一致させる**こと。値マッピングなしの `Candidates:["未着手","進行中","完了"]`（表示＝値）なら、`Statuses` 側も `DisplayText==Value` にすると揃う。
- **`SortIndexField`（NumberField）を設定しないと並べ替えの永続化が効かない**。

```json
"Statuses": { "Items": [
  { "DisplayText": "未着手", "Value": "未着手", "Color": "#6c757d", "BackgroundColor": "#DEE0E2", "CanAdd": true },
  { "DisplayText": "進行中", "Value": "進行中", "Color": "#0d6efd", "BackgroundColor": "#D9E8FF", "CanAdd": false },
  { "DisplayText": "完了",   "Value": "完了",   "Color": "#198754", "BackgroundColor": "#D5F7E7", "CanAdd": false }
] },
"StatusField": "ステータス", "SortIndexField": "並び順", "PopupLayoutName": "タスク編集"
```

---

## 3. 高さの出し方（ガント/ボード/グラフ共通）

ガント・タスクボードは `IFillHeightFieldDesign`。**フィールドを置く列の直上レイアウトを `IsFillAvailable: true` にして、フィールドをそのレイアウトの最終行に置く**だけで、親の残り高さを使い切る。GridRow 側に特別なフラグは要らない（`KeepInFillAvailableGrid` は `false` のまま。詳細は [../CommonMistakes.md](../CommonMistakes.md)・[../Layouts.md](../Layouts.md)）。

```json
"ガント行": { "Layout": {
  "IsFillAvailable": true,
  "Rows": [
    { "Columns": [ /* 戻る/タイトル等の固定高ヘッダ行 */ ] },
    { "Columns": [ { "Layout": { "FieldName": "Gantt",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign" } } ] }
  ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign" } }
```

グラフ（ApexChart 系）は行の高さで決まるので、チャートを置く `GridRow` に十分な高さを与える（タブやカードに入れるときは特に）。

> **1画面に複数の高さ可変フィールド（ガント＋ボードを縦に並べる等）を置くとき**：`IsFillAvailable` は「残り高さを使い切る」ので**1レイアウトにつき実質1フィールド**にしか効かない。ガントとボードを1詳細に同居させる、あるいはチャートを縦に複数並べる場合は、`IsFillAvailable` に頼らず**各 `GridRow` に固定の `Height`（数値）を与える**（例: ガント行 `"Height": 400`、ボード行 `"Height": 360`）。サンプル `ProjectManagementTemplate` はガントとボードを別レイアウト（別ページ）に分けているので、同居させる構成は固定 Height で組む。

---

## 4. 横断ビュー（全件まとめて見る専用画面）の制約 ⚠

「全製造オーダーの工程を1枚のガントで」「全タスクを1つのボードで」という**横断ビュー専用画面**を作りたくなる。これは:

- **表示専用モジュール（`DbTable:""`）に埋め込んでも、編集（D&D・バー移動）を保存する経路が無い**。ガント/ボードの保存は「埋め込み先の**親モジュール**の `Submit`」に依存しており、親が `DbTable:""` だと保存が走らない。
- したがって横断ビューは **「閲覧専用」と割り切る**（D&Dで動かしても永続化されない）。編集して保存させたいなら、実テーブルを持つ親レコードの詳細（第1節）に埋め込む方を主役にする。
- どうしても横断画面で保存したい場合はスクリプトで個別に保存実装が要る（標準の手本は無い＝コストとリスクを説明したうえで判断）。

---

## 5. ダッシュボード（集計クエリ＋グラフ）の定石

グラフは「**同一カテゴリ値を集計して描く**」フィールド。月別推移・ステータス別件数・不良率などは、**生テーブルを直接食わせず、SQL で GROUP BY した集計用の読み取り専用モジュール（QueryField ベース）を用意して、それをグラフのデータ元にする**。

### 構成（二層）

1. **集計クエリモジュール**（チャート1つにつき1モジュール）:
   - `DbTable:""`、`DataSourceName` は実データソース。
   - `QueryField` を1つ持ち、`{モジュール名}.{QueryField名}.sql` に `GROUP BY` の SELECT を書く。
   - クエリの出力列に対応する**フィールド**（TextField/NumberField 等）を定義する。グラフはこの**フィールドの `Name`** を参照する。
   - **ダッシュボード全体集計（入力パラメータ無し・全件 GROUP BY）の場合**：`QueryField` の `Parameters` には**出力列だけ**を `IsParameter:false` で並べる（`@xxx` のような入力パラメータは入れない）。SQL は `SELECT status AS Status, COUNT(*) AS Count FROM manufacturing_orders GROUP BY status` のように WHERE 無しでよい。（サンプルの集計クエリは親レコード絞り込み用に `@project_id` を持つが、ダッシュボードの全体集計では不要。）
2. **ダッシュボードモジュール**（`DbTable:""` の表示専用）:
   - グラフフィールドを並べる。各グラフの `SearchCondition.ModuleName` に対応する集計クエリモジュールを指定。

### グラフが参照するのは「フィールドの Name」（重要）

`CategoryField` / `Series[].Name` / `SeriesField` は、**`SearchCondition.ModuleName` のモジュールのフィールドの `Name`**（DB列名ではない）を指す。

```json
// 月別生産量（Bar）。データ元=集計クエリ MonthlyProduction（フィールド: 月, 計画数量, 実績数量）
{
  "DisplayName": "生産量推移",
  "SearchCondition": { "ModuleName": "MonthlyProduction" },
  "CategoryField": "月",
  "Series": { "Series": [
    { "Name": "計画数量", "Color": "#0d6efd", "Type": "Bar" },
    { "Name": "実績数量", "Color": "#198754", "Type": "Bar" }
  ] },
  "Name": "MonthlyChart",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexChartFieldDesign"
}
```

### チャート型ごとの系列指定の違い

| 型 | TypeFullName 末尾 | 系列プロパティ |
|---|---|---|
| 折れ線/棒/複合 | `ApexChartFieldDesign` | `Series.Series[]`（`Name` / `Color` / `Type`） |
| 横棒 | `ApexHBarChartFieldDesign` | `Series.Series[]` |
| 円/ドーナツ/ラジアル | `ApexRadialChartFieldDesign` | **`SeriesField`（単数の文字列）** |

ラジアル（円/ドーナツ）は系列配列ではなく `CategoryField`＋`SeriesField`（1つの数値フィールド名）で構成する点に注意。

---

## 6. 別レコードを「引数付きで新規作成画面」に渡す（受注→製造オーダー等）

「この受注から製造オーダーを作る」のような連携。コアの新規作成URLは `GetModuleDataUrl(モジュール, NavigationService.UrlNewId)`（`UrlNewId == "-"`）だが、**`UrlNewId` はスクリプトには公開されていない**。スクリプトからの実用パターンは次のどちらか:

- **検索/一覧画面でプレフィル**: 遷移先で `OnSearchInitialization()` 内に `NavigationService.GetQueryParameters()`（`Dictionary<string,List<string>>`）でクエリを読み、`SearchValue`/`SearchMin`/`SearchMax` に反映（在庫テンプレ `発注.mod.cs` の実績パターン）。
- **詳細新規でプレフィル**: 遷移先の `OnAfterInitialization()` で `this.IsNewData` を見て既定値をセット。クエリで値を渡すなら `NavigationService.GetUniqueQueryParameters()`（同名キーは先頭値）で読む。
- **ダイアログ方式（最も確実）**: 遷移せず、`new 製造オーダー(ModuleLayoutType.Detail)` でインスタンスを作り値をセット → `ShowDialog(...)` で確認 → `Submit()`。次節参照。

URL にクエリを付けるときは文字列連結（`url + "?key=value"`）。フレームワークも同方式。

---

## 7. 他モジュールのレコードを生成・更新して同時保存（在庫の自動増減など）

「伝票を保存したら在庫レコードも増減する」のように、**自モジュール＋他モジュールを1トランザクションで保存**するパターン。

### コンストラクタとメソッド（スクリプト公開）

```csharp
new ModuleName()                                   // 既定
new ModuleName(ModuleLayoutType.Detail)            // レイアウト種別指定
new ModuleName(ModuleLayoutType.Detail, "レイアウト名")
```

| 操作 | 戻り値 |
|---|---|
| `list.AddRow()` | 追加した行 `Module`（行型へキャストして使う） |
| `dlg.ShowDialog("OK","キャンセル")` | 押されたボタン名 `string` |
| `this.Submit()` / `this.Submit(List<Module>)` | `bool?`（`true`=成功 / `false`=中断 / `null`=キャンセル）。**必ず分岐** |

### 同時保存の意味

`this.Submit(simultaneousWriteModules)` は **`this`（自モジュール）と引数リストの全モジュールを1トランザクションで保存**する。`this` は自動で含まれるのでリストに入れなくてよい。失敗時は全ロールバック。

### 在庫反映の定石（集計→取得→更新/新規→同時保存）

```csharp
void ConfirmButton_OnClick()
{
    if (Status.Value != "未確定") return;          // 冪等性: 確定済の再実行を防ぐ
    if (!this.ValidateInput()) return;

    // 1) 明細を品目ごとに集約（入庫=+ / 出庫=-）
    var deltas = new Dictionary<long, decimal>();
    foreach (var row in (List<StockMovementLine>)Lines.Rows) {
        var sign = Kind.Value == "入庫" ? 1 : -1;
        deltas[row.Item.Value] = deltas.GetValueOrDefault(row.Item.Value) + sign * row.Quantity.Value;
    }

    // 2) 既存在庫を1クエリで取得（N+1回避）
    var inv = new ModuleSearcher<Inventory>().AddIn(e => e.Item.Value, deltas.Keys.ToList()).Search();
    var byItem = inv.ToDictionary(e => (long)e.Item.Value);

    // 3) 更新 or 新規
    var writes = new List<Module>();
    foreach (var kv in deltas) {
        if (byItem.TryGetValue(kv.Key, out var m)) { m.Count.Value += kv.Value; writes.Add(m); }
        else { var n = new Inventory(); n.Item.Value = kv.Key; n.Count.Value = kv.Value; writes.Add(n); }
    }

    // 4) 自モジュール（ステータス更新）＋在庫を1トランザクションで保存
    Status.Value = "確定済";
    this.Submit(writes);
}
```

> **二重計上の防止**: 「確定」ステータスへの更新と在庫増減を同一トランザクションにし、確定後は `OnAfterInitialization` で確定ボタンを非表示＋明細を `IsViewOnly` にして再確定不可にする。`OnTransaction` 内で在庫更新する方式もある（[Scripts.md](../Scripts.md) の `OnTransaction` 例）。

---

## 関連

- フィールド詳細: [Fields/GanttField.md](../Fields/GanttField.md) / [Fields/TaskBoardField.md](../Fields/TaskBoardField.md) / [Fields/ApexChartField.md](../Fields/ApexChartField.md) / [Fields/ApexHBarChartField.md](../Fields/ApexHBarChartField.md) / [Fields/ApexRadialChartField.md](../Fields/ApexRadialChartField.md) / [Fields/QueryField.md](../Fields/QueryField.md)
- ヘッダ＋明細の基本: [header_detail.md](header_detail.md) / [multi_nested.md](multi_nested.md)
- クエリ/SQL: [../QueryAndSql.md](../QueryAndSql.md) ／ スクリプト: [../Scripts.md](../Scripts.md)
</content>
