# DetailListField (詳細リスト)

## これは何か

**複数件のデータをカード形式（各行を詳細レイアウトで描画）で縦に並べて表示するフィールド**。各行を詳細レイアウトで描画するため、1 行あたり複数の情報をリッチに表示できます。

## いつ使うか

- 行ごとに複数の情報を表示したい時（表では横に収まらない場合）
- 写真・説明文・価格など、リッチな行を並べる場合
- カード型 UI で見栄えを整えたい場合

表形式なら [List](List.md)、タイル状なら [TileList](TileList.md) を使います。

---

## デザイナでの設定

<img src="../../Image/designer/fields/detaillist/DetailListSample_properties_panel.png" alt="DetailListFieldのプロパティパネル" style="border: 1px solid;" width="400">

### プロパティ一覧

#### システム

| C#名 | 日本語表示名 | 説明 |
|---|---|---|
| - | フィールドタイプ | `詳細リスト` 固定 |

#### 基本設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **Name** | 名前 | string | `""` | フィールド識別子 |
| **DisplayName** | 表示名 | string | `""` | 画面表示用の名前 |
| **LayoutName** | レイアウト名 | string | `""` | 各行に使う Detail レイアウト名 |
| **PagerPosition** | ページャーの位置 | enum | `Top` | ページャーの位置（`Top` / `Bottom`） |
| **UseIndexSort** | インデックスソート | bool | `false` | 表示順を Index として保存 |
| **DeleteTogether** | 親テーブルと一緒に削除 | bool | `false` | 親データ削除時に一括削除 |
| **CanCreate** | 追加 | bool | `false` | 親画面から新規作成を許可 |
| **CanUpdate** | 更新 | bool | `false` | 親画面から編集を許可 |
| **CanDelete** | 削除 | bool | `false` | 親画面から削除を許可 |
| **CanUserSort** | ユーザーソート | bool | `true` | ユーザーソートを許可 |
| **CanSelect** | 選択 | bool | `false` | 行選択を許可 |
| **OnDataChanged** | データ変更イベント | string | `""` | データ変更時のスクリプト |
| **OnSelectedIndexChanged** | 選択項目変更イベント | string | `""` | 選択行変更時のスクリプト |
| **OnSelectedIndexChanging** | 選択可否判定イベント | string | `""` | 選択行変更前のスクリプト（引数 `int index` → `bool`） |
| **OnDoubleClickRow** | 行ダブルクリックイベント | string | `""` | 行ダブルクリック時のスクリプト（引数 `int index`） |
| **ConfirmBeforeDelete** | 削除時に確認 | bool? | null | 削除前に確認ダイアログを出す |
| **IgnoreModification** | 変更判定から除外 | bool | `false` | 変更検知（IsModified）から除外 |

#### 検索設定

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **OnSearchDataChanged** | 検索モードデータ変更イベント | string | `""` | 検索条件が変更された時のスクリプト |

#### 絞り込み条件（表示データ）

| C#名 | 日本語表示名 | 型 | 既定値 | 説明 |
|---|---|---|---|---|
| **SearchCondition.ModuleName** | モジュール名 | string | `""` | 表示するデータのモジュール |
| **SearchCondition.Condition** | 抽出条件 | MultiMatchCondition | - | 絞り込み条件 |
| **SearchCondition.LimitCount** | 件数上限 | int | `50` | 表示する最大件数 |
| **SearchCondition.SortConditions** | ソート | List | `[]` | ソート順 |

> List にある `CanCustomizeColumns` / `CanNavigateToDetail` / `FormControlStyle` / `ApplyBackgroundToBoxInput` / `NavigateModuleUrlSegment` は DetailList にはありません（列の概念がカードにはないため）。

---

## スクリプトから

スクリプト API は [List](List.md#スクリプトから) と共通です（内部的にも同じ `ListField` ランタイムクラスを使用）。

- プロパティ: `Rows` / `RowCount` / `SelectedIndex` / `Page` / `PageCount` / `TotalCount` / `Limit` / `AllowLoad` / `IsValid` / `SearchComparison`
- メソッド: `AddRow` / `AddRows` / `InsertRow` / `InsertRows` / `UpdateRow` / `DeleteRow` / `DeleteAllRows` / `Reload` / `SetAdditionalCondition` / `SetSelectedIndexAsync` / `SetSearchComparisonAsync` / `ShowCustomDialog`

共通プロパティは [Field 共通プロパティ](common_properties.md) を参照。

---

## 検索での挙動

DetailListField を**検索レイアウトに配置**すると、「親レコードを、関連する子レコードの有無で絞り込む」検索 UI として機能します。

例: 顧客モジュールに注文を表示する `Orders` という DetailListField があるとき、顧客の検索画面に `Orders` を置くと、「注文を持っている顧客」「注文を持っていない顧客」を絞り込めます。

### 検索 UI

<img src="../../Image/web/fields/list/search.png" alt="DetailListField 検索 UI" style="border: 1px solid;" width="200">

ドロップダウンが 1 つだけ出ます。選択肢は次の 3 つ:

| 選択肢 | 挙動 |
|---|---|
| **（空欄）** | この条件で絞り込まない |
| **行を持つ** | 関連する子レコードが**1 件以上ある**親レコードのみ表示（SQL の `EXISTS` 相当） |
| **行を持たない** | 関連する子レコードが**1 件もない**親レコードのみ表示（SQL の `NOT EXISTS` 相当） |

### 子レコードの絞り込み条件

「どのレコードを子と見なすか」は、DetailListField のプロパティ **`SearchCondition.Condition`** で指定した条件で決まります。通常は親と子を結ぶ外部キー関係（`Orders.CustomerId == Customer.Id` 等）を設定しておきます。

`SearchCondition.Condition` に追加の条件（例: `Status == "有効"`）を入れると、それも子の絞り込みに反映されます。例えば「**有効な**注文を持っている顧客だけ表示」という検索になります。

### スクリプトから

検索 UI の状態は `SearchComparison` プロパティで読み書きできます。

```csharp
// 検索条件をプログラム的に設定
await Orders.SetSearchComparisonAsync(MatchComparison.Exists);

// 解除
await Orders.SetSearchComparisonAsync(null);
```

`SearchComparison` に設定できる値は `Exists` / `NotExists` / `null` のみです。

検索全体の仕組みは [SearchField](Search.md#検索の仕組み) / [モジュール検索設定](../module/module_search.md) を参照。

---

## 関連項目

- [List](List.md) — 表形式
- [TileList](TileList.md) — タイル形式
- [Field 共通プロパティ](common_properties.md)
