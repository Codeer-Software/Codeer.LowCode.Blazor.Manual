# ListField - データ一覧フィールド

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign`

データテーブル/グリッド。別モジュールのレコードを一覧表示し、検索・ソート・ページネーションに対応する。ListLayout を使用して列定義を行い、テーブル形式でデータを表示する。

> 行となる「行モジュール」を別途定義し、`SearchCondition.ModuleName` で参照する構成パターン（DetailListField/TileListField と行モジュールを共有する方法、デモデータの作り方）は [../RowModulePattern.md](../RowModulePattern.md) を参照。

## C# クラス定義 (真実の源)

```csharp
public class ListFieldDesign : ListFieldDesignBase, IFillHeightFieldDesign
{
    public override string LayoutName { get; set; } = "";
    public bool CanNavigateToDetail { get; set; }
    public string NavigateModuleUrlSegment { get; set; } = string.Empty;
    public bool CanCustomizeColumns { get; set; }
    public FormControlStyle? FormControlStyle { get; set; }   // enum: Box / Inline
    public bool ApplyBackgroundToBoxInput { get; set; }
    // 親 ListFieldDesignBase から継承: DisplayName, SearchCondition, PagerPosition, UseIndexSort,
    //   DeleteTogether, ReplaceMode, CanCreate, CanUpdate, CanDelete, CanUserSort, CanSelect, ConfirmBeforeDelete,
    //   OnDataChanged, OnSearchDataChanged, OnSelectedIndexChanged, OnSelectedIndexChanging, OnDoubleClickRow
    // 親 FieldDesignBase から: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。
> 一覧フィールド共通プロパティ（DisplayName, SearchCondition, PagerPosition, UseIndexSort, DeleteTogether, CanCreate, CanUpdate, CanDelete, CanUserSort, CanSelect, OnDataChanged, OnSearchDataChanged, OnSelectedIndexChanged, OnSelectedIndexChanging, OnDoubleClickRow）は [_FieldCommon.md](_FieldCommon.md) の「ListFieldDesignBase」を参照。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `LayoutName` | string | `""` | 使用するListLayout名。空の場合はデフォルトレイアウト。対象モジュール（`SearchCondition.ModuleName`）のListLayoutを指定する。 |
| `CanNavigateToDetail` | bool | `false` | `true` にすると、行クリックで詳細ページへ遷移可能になる。ListPage スコープではデフォルト `true`。 |
| `NavigateModuleUrlSegment` | string | `""` | 詳細遷移時のカスタムURLセグメント。空の場合はモジュール名がセグメントとして使われる。 |
| `CanCustomizeColumns` | bool | `false` | `true` にすると、ユーザーが表示列をカスタマイズできる。 |
| `FormControlStyle` | FormControlStyle? | `null` | フォームコントロールの表示スタイル。`Box` または `Inline`。`null` の場合はデフォルト動作。 |
| `ApplyBackgroundToBoxInput` | bool | `false` | `FormControlStyle: "Box"` のとき、行の背景色（`ListElementDesign.BackgroundColor`）を入力欄本体にも適用するか。`false` だと Box 枠の外側（td セル）のみ色が付き、中の input は透明のまま。`true` にすると input にも同じ色が伝搬する。`Inline` のときは無関係。 |

## 列挙型

### FormControlStyle

| 値 | 説明 |
|---|---|
| `Box` | ボックス表示（枠線付き） |
| `Inline` | インライン表示（枠線なし） |

## SearchCondition の使い方

`SearchCondition` でデータ取得対象と条件を定義する。

- **ModuleName:** データ取得元のモジュール名を指定する。
- **Condition.Children:** フィルタ条件を指定する。親子関係の場合、`FieldVariableMatchCondition` で親レコードのIDと子レコードの外部キーを関連付ける。
- **LimitCount:** 1ページあたりの表示件数。
- **SortConditions:** 初期ソート条件。

## 洗い替え (ReplaceMode)

Submit 時の保存方式を「差分の追加/更新/削除」から入れ替えに切り替える (`ListFieldDesignBase.ReplaceMode`)。デザイナ表示名は「洗い替え」。

| 値 | 挙動 |
|---|---|
| `None` (既定) | 通常の Add/Update/Delete |
| `All` | `SearchCondition` に一致する DB 上のデータを全件削除し、現在の行を**変更の有無に関わらず**すべて新規行として追加し直す (完全洗い替え) |
| `UpdateAsDeleteInsert` | 更新になる行 (= 既存の変更行) だけを「削除 + 新規追加」に置き換える。未変更行はそのまま、新規行は通常どおり追加、UI 上で削除された行は削除 |

```json
{ "ReplaceMode": "All", ... }
```

**用途と注意:**
- `All`: 取込データの総入れ替え、構成明細の確定保存。削除範囲は `SearchCondition` (親 Id で絞った条件) に一致する分だけ。**条件が空だとサーバー側の安全ガードで拒否される** (`SearchDelete には検索条件の指定が必要です`) ため、親子構成の ListField で使う。親が新規 (テンポラリ Id) のときは削除をスキップして追加のみ実行
- `UpdateAsDeleteInsert`: 一意制約列 (座席番号・表示順等) の**値の入れ替え**が UPDATE だと一意制約違反になるのを、Delete が先に実行されることで回避する
- どちらも作り直された行の **Id は振り直され**、`CreatedAt` / `Creator` 等の予約システムフィールドは新規としてセットし直される。子の Id を外部から FK 参照している構造では使わない
- 実装サンプル: `Samples/PatternShowcase/App/Modules/ReplaceAllSample.mod.json` (+ `ReplaceAllItem`) / `SeatReplaceSample.mod.json` (+ `Seat`)。パターン解説は [AppPatterns/replace_mode.md](../AppPatterns/replace_mode.md)

## JSON例

### 基本的な一覧表示（親子関係）

```json
{
  "LayoutName": "",
  "CanNavigateToDetail": true,
  "NavigateModuleUrlSegment": "",
  "CanCustomizeColumns": false,
  "FormControlStyle": null,
  "ApplyBackgroundToBoxInput": false,
  "DisplayName": "注文明細",
  "SearchCondition": {
    "LimitCount": 50,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "OrderItem",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [
        {
          "SearchTargetVariable": "OrderId.Value",
          "Comparison": "Equal",
          "Variable": "Id.Value",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
        }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Top",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": true,
  "CanUpdate": true,
  "CanDelete": true,
  "CanUserSort": true,
  "CanSelect": false,
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "OnSelectedIndexChanged": "",
  "OnSelectedIndexChanging": "",
  "OnDoubleClickRow": "",
  "IgnoreModification": false,
  "Name": "Items",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
}
```

### 行選択可能な一覧

```json
{
  "LayoutName": "",
  "CanNavigateToDetail": false,
  "NavigateModuleUrlSegment": "",
  "CanCustomizeColumns": true,
  "FormControlStyle": null,
  "DisplayName": "商品一覧",
  "SearchCondition": {
    "LimitCount": 20,
    "SelectFields": [],
    "SortConditions": [],
    "SortFieldVariable": "",
    "SortDescending": false,
    "ModuleName": "Product",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "PagerPosition": "Top",
  "UseIndexSort": false,
  "DeleteTogether": false,
  "CanCreate": false,
  "CanUpdate": false,
  "CanDelete": false,
  "CanUserSort": true,
  "CanSelect": true,
  "OnDataChanged": "",
  "OnSearchDataChanged": "",
  "OnSelectedIndexChanged": "ProductList_OnSelectedIndexChanged",
  "OnSelectedIndexChanging": "",
  "OnDoubleClickRow": "",
  "IgnoreModification": false,
  "Name": "ProductList",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
}
```

## スクリプトAPI

### プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| `SelectedIndex` | int | 選択中の行インデックス |
| `Page` | int (読み取り専用) | 現在のページ番号 |
| `PageCount` | int (読み取り専用) | 総ページ数 |
| `TotalCount` | int (読み取り専用) | 全行数 |
| `Rows` | List\<Module\> (読み取り専用) | 現在のページの行リスト |
| `RowCount` | int (読み取り専用) | 現在のページの行数 |
| `Limit` | int (読み取り専用) | ページサイズ |
| `AllowLoad` | bool | データ読み込みを許可するか |
| `SearchComparison` | MatchComparison? | 検索モード（Exists/NotExists） |

### メソッド

| メソッド | 説明 |
|---|---|
| `Reload()` | データを再読み込み |
| `AddRow()` | 空の行を追加 |
| `AddRow(Module src)` | モジュールから行を追加 |
| `AddRow(ModuleData src)` | データから行を追加 |
| `AddRows(int count)` | 複数の空行を追加 |
| `AddRows(List<Module> src)` | モジュールリストから複数行を追加 |
| `DeleteRow(Module row)` | 行を削除 |
| `DeleteAllRows()` | 全行削除 |
| `InsertRow(int index, Module src)` | 指定位置に行を挿入 |
| `UpdateRow(int index, Module src)` | 指定位置の行を置換 |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加検索条件を設定 |
| `ShowCustomDialog()` | カスタムダイアログ表示 |
| `Paging(int page)` | ページ遷移 |

> 共通スクリプトプロパティ（Color, BackgroundColor, IsEnabled, IsVisible, IsViewOnly 等）は [Scripts.md](../Scripts.md) を参照。

### イベントハンドラ

#### OnDataChanged

リスト内のデータが変更された時に呼ばれる。合計計算等に使用。

```csharp
// mod.json: { "OnDataChanged": "Items_OnDataChanged" }
void Items_OnDataChanged()
{
    var total = 0;
    foreach (var row in Items.Rows)
    {
        total += row.Amount.Value;
    }
    Total.Value = total;
    Tax.Value = Math.Round(Total.Value * 0.1, 0, MidpointRounding.AwayFromZero);
    TotalInTax.Value = Total.Value + Tax.Value;
}
```

#### OnSelectedIndexChanged

行選択が変更された後に呼ばれる。

```csharp
// mod.json: { "OnSelectedIndexChanged": "Items_OnSelectionChanged" }
void Items_OnSelectionChanged()
{
    var idx = Items.SelectedIndex;
    if (idx >= 0)
    {
        var row = Items.Rows[idx];
        DetailName.Value = row.Name.Value;
    }
}
```

#### OnSelectedIndexChanging

行選択が変更される前に呼ばれる。`false` を返すと選択をキャンセル。

```csharp
// mod.json: { "OnSelectedIndexChanging": "Items_OnSelectionChanging" }
bool Items_OnSelectionChanging()
{
    // 未保存の変更がある場合はキャンセル
    if (this.IsModified)
    {
        MessageBox.Show("先に保存してください");
        return false;
    }
    return true;
}
```

#### OnDoubleClickRow

行がダブルクリックされた時に呼ばれる。

```csharp
// mod.json: { "OnDoubleClickRow": "Items_OnDoubleClick" }
void Items_OnDoubleClick()
{
    var idx = Items.SelectedIndex;
    if (idx >= 0)
    {
        var row = Items.Rows[idx];
        NavigationService.NavigateTo(NavigationService.GetModuleDataUrl("Product", row.Id.Value));
    }
}
```

## ランタイム動作

- `SearchCondition` に基づいて対象モジュールからデータを取得し、`ListLayout` の列定義に従ってテーブル形式で表示する。
- ページネーションに対応。`SearchCondition.LimitCount` で1ページあたりの件数を制御する。`PagerPosition` でページャーの表示位置を指定する。
- `CanUserSort = true` の場合、列ヘッダークリックでソートが切り替わる。
- `CanNavigateToDetail = true` の場合、行クリックで詳細ページへ遷移する。`NavigateModuleUrlSegment` でURLセグメントをカスタマイズ可能。
- `CanCreate = true` で行の新規作成（`AddRowAsync`）、`CanDelete = true` で行の削除（`DeleteRowAsync`）が可能。
- `CanSelect = true` で行選択UIが表示される。選択変更時に `OnSelectedIndexChanged` イベントが発火する。
- `OnSelectedIndexChanging` で `false` を返すと選択をキャンセルできる。
- `OnDoubleClickRow` で行ダブルクリック時のカスタム処理を実行できる。

## 検索

- `Exists` / `NotExists` 比較演算子で、子レコードの存在/非存在を検索条件にできる。例えば「注文明細が存在する注文」を検索する場合に使用。

---

## DOM構造（CSS用）

### テーブル一覧

```html
<table class="table table-striped">
  <thead class="table-light">
    <tr>
      <th style="[幅・スタイル]">
        <span>列ラベル</span>
        <!-- ソート可能時 -->
        <span class="oi oi-elevator" aria-hidden="true"></span>
      </th>
    </tr>
  </thead>
  <tbody>
    <tr class="[can-select] [selected]">
      <td class="[ClassName]" style="[色・スタイル]">
        <div>
          <span style="[テキスト折り返し]">セル値</span>
        </div>
      </td>
    </tr>
  </tbody>
  <tfoot>
    <!-- CanCreate/CanDelete 時 -->
    <tr>
      <td colspan="...">
        <button class="btn btn-sm btn-outline-primary" data-system="create">新規</button>
        <button class="btn btn-sm btn-outline-danger" data-system="delete">削除</button>
      </td>
    </tr>
  </tfoot>
</table>
```

### ページャー

```html
<div class="d-flex mb-3 gap-3 align-items-center justify-content-end">
  <div class="text-end">(1-10 /100 items)</div>
  <nav role="navigation">
    <ul class="pagination m-0">
      <li class="page-item"><button class="page-link">1</button></li>
      <!-- ... -->
    </ul>
  </nav>
</div>
```

### CSSセレクタ例

```css
/* テーブルヘッダー */
[data-name="Items"] thead th {
  background-color: #2c3e50;
  color: white;
  font-weight: 500;
}

/* 選択可能行のホバー */
[data-name="Items"] tr.can-select:hover {
  background-color: #f0f7ff;
}

/* 選択中の行 */
[data-name="Items"] tr.selected {
  --bs-table-bg: #d9ecff;
}

/* セル内テキスト */
[data-name="Items"] td > div > span {
  padding: 0.5em;
}

/* フッターの操作ボタン */
[data-name="Items"] tfoot .btn {
  margin-right: 0.5rem;
}

/* ListElement.ClassName による列スタイル */
.price-column {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
```
