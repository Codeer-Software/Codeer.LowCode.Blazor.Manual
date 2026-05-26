# よくある間違いと対策

Claude Code でデザインファイルを生成する際に発生しやすい間違いのまとめ。

---

## 1. JSON の int 型プロパティに小数点付き数値を書く

**症状:** デシリアライズエラーが発生する。

**原因:** C# の `int` 型プロパティに `50.0` のような小数点付き数値を書くと、JSON パーサーが型不一致でエラーを出す。

**対象プロパティ（一部）:**
- `LimitCount` (int?)
- `FontSize` (int?)
- `ColumnSpan` (int)
- `RowSpan` (int)
- `ZIndex` (int?)
- `MaxLength` (int?)
- `Rows` (int?)
- `MaxFractionDigits` (int?)
- `TileWidth` (int)
- `MaxAllowedSize` (long?)

```json
// ❌ エラー
"LimitCount": 50.0,
"FontSize": 14.0,
"ColumnSpan": 1.0

// ✅ 正しい
"LimitCount": 50,
"FontSize": 14,
"ColumnSpan": 1
```

**対策:** 各ドキュメントのプロパティ定義にC#型を明記しているので参照すること。`int`, `int?`, `long?` 型には必ず整数値を書く。`double?`, `decimal?` 型は小数可。

---

## 2. ListLayout の Elements 構造を間違える

**症状:** 列が横に並ばず、縦に並んでしまう。

**原因:** `Elements` の構造を誤解して、各列を別々の行として配置してしまう。

```json
// ❌ 誤り: 5行ヘッダー、各1列（列が縦に並ぶ）
"Elements": [
  [{ "FieldName": "Id", "Label": "ID" }],
  [{ "FieldName": "Name", "Label": "名前" }],
  [{ "FieldName": "Price", "Label": "価格" }]
]

// ✅ 正しい: 1行ヘッダー、3列（列が横に並ぶ）
"Elements": [
  [
    { "FieldName": "Id", "Label": "ID" },
    { "FieldName": "Name", "Label": "名前" },
    { "FieldName": "Price", "Label": "価格" }
  ]
]
```

**覚え方:** `Elements[行][列]` — 外側が行、内側が列。通常は外側1要素で全列を内側に並べる。

---

## 3. LinkField の値アクセスパスを間違える

**症状:** SearchCondition や スクリプトで LinkField の値を参照できない。

**原因:** LinkField の外部キー値に `.Id.Value` でアクセスしようとする。

```
// ❌ 誤り: 存在しないパス
ExpenseReportId.Id.Value

// ✅ 正しい: LinkField 自体の値が外部キー
ExpenseReportId.Value
```

**ルール:**
- `LinkFieldName.Value` — LinkField自体の値（= DBに保存されている外部キー値）
- `LinkFieldName.ReferenceFieldName.Value` — リンク先モジュールの別フィールドの値（例: `Category.Name.Value`）

この間違いは SearchCondition の `SearchTargetVariable` / `Variable`、スクリプト、ModuleSearcher のラムダ式すべてに共通する。

---

## 4. 明細集計を DB 問い合わせで行う

**症状:** ユーザーが編集中（未保存）の明細行データが集計に反映されない。

**原因:** `ModuleSearcher` でDBから取得し直してしまう。`ModuleSearcher` はDBに保存済みのデータしか返さない。

```csharp
// ❌ 誤り: DBから取得（未保存データが反映されない）
var search = new ModuleSearcher<ExpenseItem>();
search.AddEquals(e => e.ExpenseReportId.Value, Id.Value);
var items = search.Execute();

// ✅ 正しい: 画面上のデータを使う
foreach (var row in Items.Rows)
{
    total += ((ExpenseItem)row).Amount.Value ?? 0;
}
```

**ルール:** 画面上の DetailListField / ListField のデータは `Rows` プロパティで取得。`ModuleSearcher` は別モジュールのマスタ参照等に使う。

---

## 5. IsRequired をスクリプトから設定しようとする

**症状:** 実行時エラーまたはプロパティが効かない。

**原因:** `IsRequired` はデザイン時プロパティ（JSON で定義）であり、ランタイムのスクリプトからは設定不可。

```csharp
// ❌ エラー
CustomerId.IsRequired = true;

// ✅ 正しい: IsVisible や SetError で代替
CustomerId.IsVisible = isRequired;
if (isRequired && CustomerId.Value == null)
{
    CustomerId.SetError("必須入力です");
}
```

---

## 6. LimitCount の誤設定

**症状:** 詳細画面の子リストが50件しか表示されない、または0件になる。

| 利用箇所 | 推奨値 | 理由 |
|---|---|---|
| 詳細画面の子リスト（DetailListField / ListField） | `null` | 親に紐づく子レコードは全件表示 |
| PageFrame の一覧ページ（ListPageDesign.ListFieldDesign） | `50` | ページングで件数制限 |
| LinkField / SelectField の候補検索 | `50` | 候補ダイアログでページング |

```json
// ❌ 0件になる
"LimitCount": 0

// ❌ 子リストが50件で切れる
"SearchCondition": {
  "LimitCount": 50,
  "ModuleName": "ExpenseItem",
  ...
}

// ✅ 子リストは全件表示
"SearchCondition": {
  "LimitCount": null,
  "ModuleName": "ExpenseItem",
  ...
}
```

---

## 7. QueryField の Parameters に出力列を定義しない

**症状:** クエリの結果がフィールドにマッピングされない。

**原因:** QueryField の Parameters に入力パラメータ（`IsParameter: true`）だけ定義し、SELECTの出力列（`IsParameter: false`）を定義しない。

**ルール:**
- `IsParameter: true` → SQLの `@name` にバインドされる入力パラメータ
- `IsParameter: false` → SELECTの出力列定義。`Name` はフィールドの `DbColumn` と一致させる

```json
"Parameters": [
  // 入力: SQLの @start_date にバインド
  { "IsParameter": true, "Name": "start_date", "DbType": "Date", "DbParameterDirection": "Input" },
  // 出力: SELECTの列名と対応
  { "IsParameter": false, "Name": "goods_code", "DbType": "text", "DbParameterDirection": "Input" },
  { "IsParameter": false, "Name": "total_amount", "DbType": "numeric", "DbParameterDirection": "Input" }
]
```

---

## 8. QueryField の SQLファイル名を間違える

**症状:** SQLファイルが読み込まれない。

**原因:** ファイル名が `{ModuleName}.クエリ.sql` 固定だと思い込む。実際は QueryField の `Name` プロパティに基づく。

```
ファイル名規則: {ModuleName}.{QueryFieldのName}.sql

例:
  QueryField.Name = "Query"    → Summary.Query.sql
  QueryField.Name = "クエリ"    → Summary.クエリ.sql
```

---

## 9. クエリ専用モジュールに Id フィールドや DbTable を設定する

**症状:** 不要なDB操作が発生する、またはエラーになる。

**QueryField を使うクエリ専用モジュールの構成:**
- `DbTable`: 空文字 `""` にする
- `CanCreate`/`CanUpdate`/`CanDelete`: `false`
- `Id` フィールド: 不要（定義しない）
- 各フィールドの `DbColumn`: Parameters の出力列 `Name` と一致させる

---

## 10. ラベルと入力フィールドの上下配置で直接2行にする

**症状:** レイアウトが崩れる、ラベルとフィールドの間に余計な余白が入る。

**原因:** 外側の行に直接2行で並べてしまう。

**正しい方法:** カラム内にネストした GridLayoutDesign を使い、ラベル行の `Margin.Bottom` を `0` にする。

詳細は [LayoutGuidelines.md](LayoutGuidelines.md) の「DetailLayout - ラベルを上に配置する場合」を参照。

---

## 11. DetailListField の子モジュールに IsBordered を設定しない

**症状:** 先頭フィールドにフォーカスを当てたときにハイライトの上部が見切れる。

**対策:** 子モジュールの DetailLayout の `GridLayoutDesign` に `IsBordered: true` を設定する。

詳細は [LayoutGuidelines.md](LayoutGuidelines.md) の「DetailLayout - DetailListField に入れるモジュール」を参照。

---

## 12. スクリプトで this を使わずにモジュールメソッドを呼ぶ

**症状:** `Submit()` や `ValidateInput()` が見つからないエラー。

**ルール:** モジュール自体のメソッドは `this.` を付けて呼ぶ。フィールドへのアクセスは `this.` 不要。

```csharp
// ❌ 誤り
Submit();
ValidateInput();

// ✅ 正しい
this.Submit();
this.ValidateInput();

// フィールドは this 不要
Name.Value = "テスト";
```

---

## 13. SearchCondition で LimitCount=2147483647 を使う

**旧方式:** 以前は `LimitCount` のデフォルトが `int.MaxValue`（2147483647）だった。

**現方式:** `LimitCount` は `int?` 型。全件取得する場合は `null` を使う。

```json
// ❌ 旧方式（動作はするが冗長）
"LimitCount": 2147483647

// ✅ 現方式
"LimitCount": null
```

---

## 14. 入力フィールドの GridColumn に HorizontalAlignment を設定する

**症状:** 入力欄の横幅が崩れる（入力フィールドが小さくなる）。

**ルール:** 入力フィールド（TextField, NumberField, DateField, SelectField 等）の GridColumn では `HorizontalAlignment` を設定しない（省略する）。ラベルやボタンには設定OK。

詳細は [LayoutGuidelines.md](LayoutGuidelines.md) を参照。

---

## 15. SortConditions と旧方式 SortFieldVariable を混同する

SearchCondition のソート指定は2つの方式がある。**新方式 `SortConditions` を使うこと。**

```json
// ❌ 旧方式（非推奨）
"SortFieldVariable": "CreatedDate.Value",
"SortDescending": true

// ✅ 新方式: 複数ソート条件が可能
"SortConditions": [
  { "Variable": "CreatedDate.Value", "IsDescending": true },
  { "Variable": "Name.Value", "IsDescending": false }
]
```

---

## 16. AnchorTagField の Target に "Module" や "PageFrame" を指定する

**症状:** デシリアライズエラー、またはナビゲーションが動作しない。

**原因:** `Target` プロパティの型は `AnchorTarget` 列挙型で、有効な値は `Url` / `HistoryBack` / `HistoryForward` の3つのみ。`Module` や `PageFrame` は `Target` の値ではなく、別のプロパティ。

```json
// ❌ 誤り: "Module" は AnchorTarget 列挙型の有効な値ではない
"Target": "Module"

// ✅ 正しい: Target は "Url" で、Module プロパティでナビゲーション先を指定
"Target": "Url",
"Module": "ProductDetail",
"IdVariable": "Id.Value"
```

**ルール:**
- モジュール遷移: `Target: "Url"` + `Module` プロパティ
- ブラウザ戻る: `Target: "HistoryBack"`
- 外部URL: `Target: "Url"` + `Url` プロパティ

---

## 17. IsViewOnly をフィールド定義に設定する

**症状:** フィールドが閲覧専用にならない。

**原因:** `IsViewOnly` をモジュールの Fields 配列内のフィールド定義に設定している。

**ルール:** `IsViewOnly` はフィールド定義のプロパティではなく、**レイアウト要素**のプロパティ。

- ListLayout の ListElement に設定
- DetailLayout の FieldLayoutDesign / GridLayoutDesign に設定

```json
// ❌ 誤り: フィールド定義に IsViewOnly を設定
"Fields": [
  {
    "DbColumn": "合計",
    "IsViewOnly": true,
    "Name": "合計",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
  }
]

// ✅ 正しい: ListElement に設定
"Elements": [
  [
    {
      "FieldName": "合計",
      "Label": "合計",
      "IsViewOnly": true
    }
  ]
]
```

---

## 18. 一覧ページのソートをモジュールの ListPageFieldDesign に設定する

**症状:** 一覧ページでソートが効かない。

**原因:** モジュール定義ファイル (`.mod.json`) の `ListPageFieldDesign.SearchCondition.SortConditions` に設定している。

**ルール:** 一覧ページとして表示する場合のソートは、**PageFrame** (`*.frm.json`) のリンク内にある `ListFieldDesign.SearchCondition.SortConditions` で設定する。

モジュール側の `ListPageFieldDesign` はモジュール自体のデフォルト設定だが、PageFrame のリンク設定が優先される。

```json
// ❌ 誤り: モジュールの ListPageFieldDesign に設定
// 商品.mod.json
"ListPageFieldDesign": {
  "SearchCondition": {
    "SortConditions": [{"Variable": "Id.Value", "IsDescending": false}]
  }
}

// ✅ 正しい: PageFrame のリンク内に設定
// Main.frm.json
"Links": [
  {
    "Module": "商品",
    "ListPageDesign": {
      "ListFieldDesign": {
        "SearchCondition": {
          "SortConditions": [{"Variable": "Id.Value", "IsDescending": false}]
        }
      }
    }
  }
]
```

**注意:** DetailListField / ListField（モジュール内の子リスト）のソートは、フィールド自身の `SearchCondition.SortConditions` で設定する。これはモジュール定義側で正しい。

---

## 19. フィールドの型変更後に IsRequired が残る

**症状:** `ValidateInput()` が失敗してデータが保存できない。

**原因:** フィールドの型や用途を変更した際に、元の `IsRequired: true` 設定が残ったまま。直接編集されなくなったフィールド（LinkFieldNames 経由の表示専用等）に必須チェックが効いてしまう。

```json
// ❌ 誤り: 単価をLinkFieldNames経由に変更したが IsRequired: true が残っている
{
  "DbColumn": "単価",
  "IsRequired": true,
  "Name": "単価",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}

// ✅ 正しい: 直接編集しないフィールドは IsRequired: false にする
{
  "DbColumn": "単価",
  "IsRequired": false,
  "Name": "単価",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}
```

**ルール:** フィールドの型や用途を変更したとき、`IsRequired` 等の既存設定が新しい用途と矛盾しないか確認する。

---

## 20. LinkFieldNames のパス書式を間違える

**症状:** リンク先のフィールド値が表示されない。

**原因:** `LinkFieldNames` の書式が正しくない。

**ルール:** `LinkFieldNames` の各エントリは `"LinkFieldのName.参照先のFieldName"` の形式。

```json
// ❌ 誤り: LinkField名ではなくモジュール名を使っている
"LinkFieldNames": ["Author.Fullname"]
// ↑ "Author" はモジュール名ではなく、このモジュール内の LinkField の Name でなければならない

// ❌ 誤り: 3階層のパスを書いている
"LinkFieldNames": ["AuthorLink.Author.Fullname"]

// ✅ 正しい: LinkField名.参照先フィールド名
// (この例では LinkField の Name が "Author" で、参照先モジュールの "Fullname" フィールドを取得)
"LinkFieldNames": ["Author.Fullname"]
```

**注意:** LinkField の `Name` と参照先モジュールの `Name` がたまたま同じ場合もある（例: `Author` という名前の LinkField が `Author` モジュールを参照）。混乱しやすいが、ここに書くのは常に **このモジュール内の LinkField の Name** である。

---

## 21. IsUpdateProtected と IsViewOnly を混同する

**症状:** 意図した動作と異なる。

**原因:** `IsUpdateProtected` と `IsViewOnly` は異なるプロパティ。

| プロパティ | 設定場所 | 動作 |
|---|---|---|
| `IsUpdateProtected` | フィールド定義 (JSON) | **読み取り専用**。サーバーサイドでもチェックが入り、API経由での更新も防止される。 |
| `IsViewOnly` | レイアウト要素 (JSON) / スクリプト | **表示上の読み取り専用**。UI上では編集不可だが、サーバーサイドの強制力はない。 |

```json
// IsUpdateProtected: フィールド定義に設定（サーバーサイドでも更新防止）
{
  "DbColumn": "code",
  "IsUpdateProtected": true,
  "Name": "Code",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign"
}

// IsViewOnly: レイアウト要素に設定（表示上の読み取り専用）
{
  "FieldName": "Code",
  "IsViewOnly": true,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
}
```

**使い分け:**
- DBから値を取得するだけで、このモジュールでは変更しないフィールド → `IsUpdateProtected: true`
- 計算結果や参照値のように「画面上で表示のみ」→ レイアウトの `IsViewOnly: true`

---

## 22. 表示専用モジュールに ListLayout / SubmitButton / Id を設定する

**症状:** 表示専用モジュール（チャートダッシュボード等）で不要な一覧画面が表示される、または不要なSubmitボタンが表示される。

**原因:** `DbTable` が空のモジュール（DB連携なし）に、CRUD用のモジュールと同じ構成（ListLayout, SubmitButton, Id）を設定してしまう。

**ルール:** `DbTable` が空の表示専用モジュールでは:
- `ListLayouts` の Elements にフィールドを入れない — フィールドを入れると一覧表示モジュールと認識されてしまう
- `SubmitButtonFieldDesign` を含めない — データ保存が不要
- `IdFieldDesign` を含めない — 主キーが不要

```json
// ❌ 誤り: 表示専用モジュールにCRUD構成を設定
{
  "Name": "SalesDashboard",
  "DbTable": "",
  "Fields": [
    { "Name": "Id", "TypeFullName": "...IdFieldDesign" },
    { "Name": "Chart", "TypeFullName": "...ApexChartFieldDesign", "..." : "..." },
    { "Name": "Submit", "TypeFullName": "...SubmitButtonFieldDesign" }
  ],
  "ListLayouts": { "": { "Elements": [[{ "FieldName": "Chart" }]] } }
}

// ✅ 正しい: 表示専用モジュールはチャートフィールドのみ、ListLayoutのElementsは空
{
  "Name": "SalesDashboard",
  "DbTable": "",
  "DataSourceName": "",
  "Fields": [
    { "Name": "SalesChart", "TypeFullName": "...ApexChartFieldDesign", "..." : "..." }
  ]
}
```

---

## 23. 親子関係の外部キーに NumberField を使う

**症状:** 新規作成した親レコードに紐づく子レコードが保存できない、またはデータの関連付けが壊れる。

**原因:** DetailListField の子モジュールで、親の ID を参照する外部キーフィールドに `NumberFieldDesign` を使っている。親レコードが未保存の状態では、フレームワーク内部でテンポラリ ID（文字列）が一時的に割り当てられる。NumberField は数値しか保持できないため、テンポラリ ID を正しく扱えない。

```json
// ❌ 誤り: NumberField で親IDを持つ
{
  "DbColumn": "parent_id",
  "Name": "ParentId",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
}

// ✅ 正しい: LinkField で親IDを持つ
{
  "DbColumn": "parent_id",
  "SearchCondition": {
    "LimitCount": 50,
    "ModuleName": "ParentModule",
    "Condition": {
      "IsOrMatch": false,
      "IsNot": false,
      "Children": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "ValueVariable": "Id.Value",
  "DisplayTextVariable": "Title.Value",
  "Name": "ParentId",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign"
}
```

**ルール:** 外部キーフィールドには `LinkFieldDesign` または `IdFieldDesign` を使う。DB カラムの型は実際の ID 型（通常は INTEGER）のままでよい。テンポラリ ID はアプリ内部の話であり、DB 保存時には実際の ID に変換される。

---

## 24. スクリプトでNull条件演算子（?.）を使う

**症状:** スクリプト実行時に InvalidSyntax エラーが発生する。

**原因:** Null条件演算子（`?.`、`?[]`）はスクリプトエンジンでサポートされていない。

```csharp
// ❌ エラー: ?. はサポートされない
var name = product?.Name.Value;
var first = list?[0];

// ✅ 正しい: nullチェックを明示的に書く
string name = null;
if (product != null)
{
    name = product.Name.Value;
}

// ✅ 正しい: ?? は使える
var value = Price.Value ?? 0;
```

**ルール:** `?.` と `?[]` の代わりに `if (x != null)` で明示的にnullチェックする。Null合体演算子（`??`）は使用可能。

---

## 25. FieldLayoutDesign に IsViewOnly を設定し忘れる

**症状:** 詳細画面でフィールドが編集可能なまま。

**原因:** `IsViewOnly` は FieldLayoutDesign のプロパティとして存在するが、デフォルトでは省略されることが多く、設定を忘れやすい。特に LinkFieldNames 経由のフィールドで忘れがち。

```json
// ❌ IsViewOnly なし: 編集可能になってしまう
{
  "FieldName": "Author.Fullname",
  "Name": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
}

// ✅ IsViewOnly: true を設定
{
  "FieldName": "Author.Fullname",
  "IsViewOnly": true,
  "Name": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
}
```

**または、GridLayoutDesign 全体を閲覧専用にする:**
```json
{
  "Rows": [...],
  "IsViewOnly": true,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
}
```

---

## 26. リストのチェックボックスに CanSelect を使う

**症状:** `CanSelect: true` を設定してリストにチェックボックスを付けようとするが、意図した動作にならない。

**原因:** `CanSelect` は行の選択状態（ハイライト）を制御するフラグであり、チェックボックスUIではない。

**対策:** チェックボックスを実装するには:

1. 対象モジュールに `BooleanFieldDesign`（`UIType: "CheckBox"`）を追加
2. `ListLayout` の `Elements` の先頭列にそのフィールドを配置
3. そのListElementに `IsViewOnly: false` を設定（他の列は `IsViewOnly: true`）
4. ListElementの `Label` にはスペース `" "` を設定（空文字だとヘッダーが詰まる）
5. `CanSelect` は `false` にする（選択ハイライトとチェックボックスが混在すると紛らわしい）

```json
// Fields に追加
{
  "DbColumn": "",
  "Text": "",
  "UIType": "CheckBox",
  "Name": "Selected",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}

// ListLayout Elements の先頭に追加
{
  "FieldName": "Selected",
  "Label": " ",
  "IsViewOnly": false,
  "ColumnSpan": 1,
  "RowSpan": 1,
  "CanResize": false,
  "CanUserSort": false
}

// ListFieldDesign
{ "CanSelect": false }
```

---

## 27. 表示専用モジュールで入力可能なリストを使うときに CanUpdate を false にする

**症状:** 表示専用モジュール（`DbTable: ""`）のDetailLayoutに配置したListFieldで、`IsViewOnly: false` のカラムが編集できない。画面全体がViewOnly状態になる。

**原因:** モジュールの `CanUpdate` が `false` だと、画面全体が読み取り専用モードになり、個別フィールドの `IsViewOnly: false` が効かない。

**対策:** 表示専用モジュールでも、リスト内に入力可能なフィールドがある場合は `CanUpdate: true` を設定する。

```json
{
  "Name": "PriceTag",
  "DataSourceName": "",
  "DbTable": "",
  "CanCreate": false,
  "CanUpdate": true,   // ← これがないとリスト内の入力が効かない
  "CanDelete": false,
  ...
}
```

`DbTable` が空でも `CanUpdate: true` は問題ない。DB書き込みは発生しないが、画面のViewOnly制御が解除される。

---

## 28. FieldValueMatchCondition の Value に TypeFullName を指定しない

**症状:** モジュールの読み込みに失敗する。「TypeFullName property is missing」エラー。

**原因:** `FieldValueMatchCondition` の `Value` プロパティの型は `MultiTypeValue`（抽象クラス、`JsonAbstract` 継承）である。`JsonAbstract` を継承するクラスはすべて JSON に `TypeFullName` が必須。

**間違い:**
```json
{
  "SearchTargetVariable": "PartnerCode.Value",
  "Comparison": "Equal",
  "Value": {
    "Value": "AH"
  },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
}
```

**正しい:**
```json
{
  "SearchTargetVariable": "PartnerCode.Value",
  "Comparison": "Equal",
  "Value": {
    "Value": "AH",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.StringValue"
  },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldValueMatchCondition"
}
```

**MultiTypeValue の派生クラス一覧:**

| クラス | TypeFullName | 用途 |
|---|---|---|
| `StringValue` | `Codeer.LowCode.Blazor.Repository.StringValue` | 文字列値 |
| `DecimalValue` | `Codeer.LowCode.Blazor.Repository.DecimalValue` | 数値 |
| `BooleanValue` | `Codeer.LowCode.Blazor.Repository.BooleanValue` | 真偽値 |
| `NullValue` | `Codeer.LowCode.Blazor.Repository.NullValue` | null |

**ルール:** `JsonAbstract` を継承するクラスのオブジェクトは、**どんな場所に出現しても必ず `TypeFullName` を含める必要がある**。これは JSON デシリアライザ（`JsonAbstractClassConverter`）が具象型を判別するために使用する。

---

## 29. ViewEditToggleButton と SubmitButton の関係を誤解する

ViewEditToggleButtonField をモジュールに配置すると、**初期表示で同一モジュール内の全 SubmitButton が自動的に非表示**になる。仕様通りの挙動だが、知らないと「Submit ボタンが消えた」バグに見える。

### 挙動

1. 初期化時: モジュールが `IsViewOnly = true`（表示モード）で開く
2. 初期化時: 全 `SubmitButtonField` の `IsVisible` が `false` になる
3. ViewEditButton クリック → 編集モード: SubmitButton が `IsVisible = true` に戻る
4. 再度クリック → 表示モード: SubmitButton が再度非表示 + データ再読込

### 対処

- ViewEditToggleButton は **SubmitButton との併用が前提**。どちらか一方だけ配置する設計にしない
- 動作確認時は「編集モードに切り替えてから Submit を確認する」動線で見る
- 表示専用モジュール（`DbTable` 空）では原則使わない（編集モードに意味がない）

詳細は [Fields/ViewEditToggleButtonField.md](Fields/ViewEditToggleButtonField.md) の「重要な副作用」セクション参照。

---

## 30. 存在しない Module API を使う（ReloadWithLock など）

過去のサンプルやテストデータに `module.ReloadWithLock()` を使ったコードが残っているが、**現在の公開 API には `ReloadWithLock()` は存在しない**。実行時にエラーになる。

```csharp
// ❌ 誤り: 公開API に存在しない
stock.ReloadWithLock();

// ✅ 正しい: Reload() を使う
stock.Reload();
```

`Source/TestData/Demo20231211/` 配下のスクリプトサンプルで `ReloadWithLock()` の使用例が残っているが、これは**過去バージョンの遺物**。コピーして使わないこと。

ロック付き再読込が本当に必要な場合は、サーバー側の SQL でロック処理を行う（`ExecuteSqlField` で `SELECT ... FOR UPDATE` 等）か、トランザクション内で再読込する設計を検討する。

---

## 31. FontWeight / FontStyle がカスケードしない

`FieldLayoutDesign` および各レイアウトの `FontWeight` / `FontStyle` は、**親レイアウトに値があっても子に継承されない**。明示的に指定したフィールドにだけ適用される。

カスケード対象は `Color` / `BackgroundColor` / `FontFamily` / `FontSize` の **4 つだけ**。太字や斜体を全体に効かせたい場合は、対象フィールドを 1 つずつ指定するか、`app.css` で CSS から指定する。

```css
/* app.css で全体を太字にする */
.field-layout {
  font-weight: 600;
}
```

---

## 32. CanvasElement に Name を付けようとする

`CanvasElement` には `Name` プロパティが存在しない。スクリプトから個別の Element を直接操作する手段はない。

```json
// ❌ 誤り: Name プロパティは存在しない
{
  "Name": "MyElement",
  "Left": 10,
  "Top": 10,
  "Width": 200
}
```

スクリプトから操作したい場合は、Element の中に Layout（GridLayoutDesign / TabLayoutDesign / 別の CanvasLayoutDesign）をネストし、その Layout の `Name` 経由でアクセスする。

```json
// ✅ 正しい: 中の Layout に Name を付ける
{
  "Layout": {
    "Name": "MyInnerGrid",
    "Rows": [...],
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
  },
  "Left": 10,
  "Top": 10,
  "Width": 200
}
```

---

## 33. ID の DB 列を `TEXT`/`VARCHAR` で GUID 保存する設計にする

**症状:** `CREATE TABLE` で `id TEXT PRIMARY KEY`（あるいは `VARCHAR(36)`）を出力し、各行に GUID 文字列を保存するスキーマを生成してしまう。

**原因:** `IdField` のスクリプト API `Value` の型が `string?` であること、ランタイム説明に「UUID を自動生成」とあることから、ID は文字列型と誤解しがち。

**実態:** UUID は **新規作成中のメモリ上だけのテンポラリ ID**。DB に保存される時点で DB 側採番の `long`（INTEGER）値に置き換わる。DB 列は数値型 + 自動採番が原則。

```sql
-- ❌ 間違い: GUID を DB に保存する設計
CREATE TABLE customer (
    id TEXT PRIMARY KEY,
    name TEXT
);

-- ✅ 正しい: long 自動採番
CREATE TABLE customer (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT
);
```

**例外:** `IdField.IsManualInput = true` で社員番号 `EMP-001` 等の業務 ID を扱うケースは TEXT/VARCHAR 列でよい。デフォルトの自動採番ケースでは絶対 `INTEGER`。

詳細は [DatabaseGuidelines.md](DatabaseGuidelines.md) と [Fields/IdField.md](Fields/IdField.md) を参照。

---

## 34. 存在しない FieldDesign クラスを使う（CreatedByFieldDesign など）

**症状:** モジュール JSON を読み込んだ瞬間に `JsonException: Type ... is not a valid FieldDesignBase type.` で読み込みが失敗し、デザイナのモジュール一覧から消える。サイドバーリンクからも辿れなくなる。

**原因:** 「作成者」「更新日時」「画像」「ネスト Module」のような“ありそうな” 名前のクラスを推測で書いてしまうが、CLB に実在しない。実在クラスは `Source/Codeer.LowCode.Blazor/Repository/Design/*FieldDesign.cs` を grep して確認するか、[Fields/](Fields/) ディレクトリの個別 `.md` を参照すること。

**実在しない名前と置き換え先 (代表例):**

| ❌ 存在しない | ✅ 実在クラス・代替 |
|---|---|
| `CreatedByFieldDesign` / `UpdatedByFieldDesign` | 普通の `TextFieldDesign` + `IgnoreModification: true` + `IsUpdateProtected: true`、サーバ/スクリプト側で値をセット |
| `CreatedDateTimeFieldDesign` / `UpdatedDateTimeFieldDesign` | 普通の `DateTimeFieldDesign` + 同上 |
| `ChildModuleFieldDesign` | `ModuleFieldDesign` (他モジュール埋め込み) |
| `ImageFieldDesign` | `FileFieldDesign` + `AcceptedExtensions: ".png,.jpg,.jpeg,.gif"`、表示専用なら `ImageViewerFieldDesign` |

**対策:**

- JSON を書く前に [Fields/](Fields/) もしくは `Source/Codeer.LowCode.Blazor/Repository/Design/` の実クラスを必ず確認する。
- Python スクリプト等で機械生成する場合は、TypeFullName の文字列を一覧で先に確認してから書き出す。
- 1 つでもこのエラーが出ると、PageFrame の `Module 'X' が存在しません` エラーが連鎖して出るので、デザインチェックでまず `JsonException` のあるモジュール名を直す。

詳細な実在クラス一覧は [Fields/](Fields/) ディレクトリ全体と、各 `*FieldDesign.cs` のソースを参照。

---

## 35. 一覧ページで編集可能にしてしまう (PageFrame の CRUD 設定)

**症状:** PageFrame の `Auto`/`ListToDetail` 設定で一覧画面に行内編集 UI が出てしまい、新規作成・更新を一覧上で直接やる UX になる。「一覧から行クリック → 詳細で編集」の一般的なフローと違って違和感がある。

**原因:** `Auto` 系のリンクで `ListPageDesign.ListFieldDesign` の `CanCreate: true` / `CanUpdate: true` を一律に立ててしまっている。これは一覧画面の **テーブル内** で直接編集する設定で、一般的な業務アプリの UX とは違う。

**一般的な CRUD 画面の設定:**

```json
"ListPageDesign": {
  "UseNavigateToCreate": true,            // 一覧の上部に「新規作成」ボタンを出す → 詳細画面へ
  "ListFieldDesign": {
    "CanNavigateToDetail": true,          // 行クリックで詳細画面に遷移
    "CanCreate": false,                   // 一覧テーブル内での新規作成は無効
    "CanUpdate": false,                   // 一覧テーブル内での編集は無効
    "CanDelete": true,                    // 削除は一覧から (確認ダイアログ後)
    "CanUserSort": true,
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
  }
}
```

**フロー:** 一覧 → 「新規作成」ボタンで詳細画面に遷移 → 詳細で入力 → SubmitButton で保存。
一覧では編集せず閲覧と削除のみ。

**例外 (一覧で直接編集を見せたいケース):**

- スプレッドシート風の大量データ修正
- ピッキングリスト等の操作画面
- これらは明示的に `CanUpdate: true` を入れて意図を示す。デフォルトの「Auto リンク」では入れない。

**`CanBulkDataUpdate` / `CanBulkDataDownload` は別物:** CSV インポート/エクスポート向けの一括処理機能で、Auto リンクで自動有効にしない (個別 mod の意図に応じて立てる)。

詳細は [PageFrame.md](PageFrame.md) の ListPageDesign セクションを参照。

---

## 36. 一覧→詳細フローの詳細画面に「戻る」ボタンを置く

**症状:** CRUD 系の一覧画面から行クリックで詳細画面に遷移した後、ブラウザの戻るボタンしか戻る手段がない (UX が悪い)。

**原因:** 詳細レイアウトに「戻る」UI を入れ忘れ。詳細画面は単独で完結する作りにしがちで、ナビゲーションを忘れる。

**標準パターン (GettingStarted の `Author` モジュールを参考):**

詳細レイアウトの **1 行目に AnchorTagField (HistoryBack)** を配置する。アイコンは Bootstrap Icons の `bi bi-arrow-left-circle-fill`、FontSize 30 で大きめに。

**Field 定義:**
```json
{
  "Style": "Text",
  "Target": "HistoryBack",
  "ShouldOpenInNewTab": false,
  "Icon": "bi bi-arrow-left-circle-fill",
  "TitleText": "",
  "TitleVariable": "",
  "ImageResourcePath": "",
  "PageFrame": "",
  "Module": "",
  "ModuleVariable": "",
  "IdVariable": "",
  "Url": "",
  "OnClick": "",
  "Name": "BackButton",
  "IgnoreModification": false,
  "OnValidateInput": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.AnchorTagFieldDesign"
}
```

**Layout (DetailLayouts の Rows 先頭) :**
```json
{
  "IsWrap": false, "Margin": {}, "GridRowType": "Normal",
  "Columns": [{
    "Layout": {
      "FieldName": "BackButton",
      "FontSize": 30,
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
    },
    "HorizontalAlignment": "Start",
    "BorderStyle": { "LeftColor": "", "TopColor": "", "RightColor": "", "BottomColor": "" },
    "Border": "None"
  }]
}
```

**ポイント:**
- `Target: "HistoryBack"` は **ブラウザ履歴を1つ戻る** 動作 (一覧画面に戻る)。`AnchorTagField.Target` は `Url` / `HistoryBack` / `HistoryForward` のみ有効。
- `Style: "Text"` でアイコンを表示 (アイコン or テキスト or 画像が選べる)
- `HorizontalAlignment: "Start"` で左寄せ
- `FontSize: 30` でアイコンを大きく表示

**戻る不要のケース:**
- ダッシュボードや Home モジュール (`ModulePageType: Detail` の TopPage)
- ダイアログ・ポップアップで開く詳細 (閉じるボタンで戻る)
- サブフレーム内のページで親フレームの SubmitButton 経由で戻る場合

詳細は [Fields/AnchorTagField.md](Fields/AnchorTagField.md) を参照。

---

## 37. デザイン系を新規作成するときはC#プロパティのデフォルト値を踏襲する (一番大事)

**症状:** Module / Field / Layout / PageFrame / PageLink などを新規生成する際、推測でプロパティ値を書くと「画面が ViewOnly になる」「リストの新規ボタンが消える」「リンクが意図したページに飛ばない」など、説明しにくい挙動になる。

**原因:** C# のプロパティに設定されている初期値を確認せずに、それっぽい値を書いてしまう。たとえば `CanCreate / CanUpdate / CanDelete` は **デフォルト `true`** だが、表示専用モジュールっぽいので一律 `false` にすると、入力フィールドが ViewOnly になって編集できなくなる。

**鉄則: デフォルト値が基本。確たる理由がない限りデフォルト値のまま生成する。**

JSON 上は「省略 = デフォルト値」と「明示的にデフォルト値を書く」のどちらでも動くが、デザイナで保存し直すと省略していたキーがフル展開されるので、生成時に値を書く場合は **必ず C# のフィールド初期値と一致させる**。

### 主な例: ModuleDesign の CRUD フラグ

```csharp
// Source/Codeer.LowCode.Blazor/Repository/Design/ModuleDesign.cs
public bool CanCreate { get; set; } = true;   // デフォルト true
public bool CanUpdate { get; set; } = true;   // デフォルト true
public bool CanDelete { get; set; } = true;   // デフォルト true
```

→ Module を新規生成するときは **基本 `true` 3つ揃え**で書く。`DbTable` が空の表示専用モジュールであっても、入力フィールドが画面にあるなら `CanUpdate: true` にしないとフィールドが ViewOnly になる ([#27 参照](#27-表示専用モジュールでも入力があるなら-canupdate-true))。

「確たる理由」の例 (= ここだけ `false` にしてもいい):
- 監査ログ・履歴の閲覧専用画面 → `CanCreate/Update/Delete: false`
- 一覧してデータを表示するだけのレポート画面
- システム管理者しか作らないマスタを一般ユーザーから隠したい

### デフォルト値の確認手順

1. 当該クラスのソース (例: `Source/Codeer.LowCode.Blazor/Repository/Design/{XxxDesign}.cs`) を開く
2. プロパティの `= ...` の初期値を確認
3. `[Designer(...)]` 属性にデフォルト挙動のヒントが書いてあることがある
4. 初期値が未指定 (`string Name { get; set; }`) → C# 既定 (`""` / `0` / `false` / `null`)

### このルールの適用範囲

- ModuleDesign (CRUD 3 フラグ、`CanBulkDataUpdate`/`CanBulkDataDownload` 等)
- FieldDesign (`IsUpdateProtected`, `IsRequired`, `IgnoreModification` などすべて)
- LayoutDesign (`IsBordered`, `IsFlowLayout`, `IsAutoFillWrap`, `IsFillAvailable`, `IsRowMarginRemoved` 等)
- PageFrameDesign / SideBarDesign / HeaderDesign (`IsVisible` のデフォルトに注意 — 表示するなら明示的に `true`)
- PageLink / ModulePageDesign (`ModulePageType` のデフォルト, `ListPageDesign.UseNavigateToCreate` のデフォルト等)
- 各 GridColumn / GridRow / FieldLayout のプロパティ

迷ったらまず `Source/Codeer.LowCode.Blazor/Repository/Design/` のクラスファイルを見る。「自分の意図」より「C# が出すデフォルト」を尊重する。

---

## 38. PageFrame に登録されてないモジュールは画面が真っ白になる

**症状:** 親モジュールの ListField から行クリックで子モジュールの詳細に遷移しようとすると、URL は変わるが画面が真っ白で何も表示されない。あるいはダイアログ用モジュールを `ShowDialog` で開いても出ない。

**原因:** PageFrame の `Left.Links` (サイドバー) にも `OtherPageModuleDesigns` (その他のページ) にも未登録のモジュールは、CLB がレンダリングしない。サイドバーには出したくないけど詳細遷移先・ダイアログ先として使うモジュールは、**`OtherPageModuleDesigns` に登録**が必要。

**該当するモジュール:**
- ヘッダ-詳細パターンの **子テーブル** (例: `OrderDetail`, `Phase`, `Task`)
- リンク参照される **マスタテーブル** (例: `Tag`, `LookupCustomer`)
- ダイアログ専用モジュール (例: `EditDialogTarget`)
- コメント・履歴の **子テーブル** (例: `Comment`)
- ※ `ChildModuleFieldDesign` (= `ModuleFieldDesign`) で**埋め込み**するだけのモジュールは画面遷移しないので **登録不要**

**JSON 構造 (PageLink から `Title/Icon/IconType/HideTitle` を抜いたもの):**

```json
"OtherPageModuleDesigns": [
  {
    "ModulePageType": "Auto",
    "ModuleUrlSegment": "",
    "ActiveModuleSegments": [],
    "PageFrame": "",
    "Module": "OrderDetail",
    "Id": "",
    "Parameters": "",
    "ListPageDesign": {
      "SearchLayoutName": "",
      "UserUrlParameter": true,
      "UseNavigateToCreate": true,
      "ListFieldDesign": {
        "LayoutName": "",
        "CanNavigateToDetail": true,
        "SearchCondition": {
          "LimitCount": 50,
          "ModuleName": "OrderDetail"
        },
        "CanDelete": true, "CanUserSort": true,
        "Name": "",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
      }
    },
    "DetailPageDesign": { "PageTitle": "", "LayoutName": "", "UrlParameter": "" }
  }
]
```

**ポイント:**
- `Module` 名を指定するだけで、デフォルトの一覧/詳細レイアウトが描画される
- ダイアログ用 (Detail のみ表示) なら `ModulePageType: "Detail"`、通常の CRUD 系は `Auto`
- `SearchCondition.ModuleName` も該当モジュール名で揃える ([#?? 参照](#))

詳細は [PageFrame.md](PageFrame.md) の `OtherPageModuleDesigns` セクションを参照。

---

## 39. SearchLayouts は空辞書 `{}` ではなく、最低限 `""` キーの空レイアウトを作る

**症状:** 一覧ページを開くと `モジュール名が一致しません` の `LowCodeException` が出て真っ白になる。スタックトレースは `ListField.SetAdditionalConditionAsync` → `SearchField.InvokeOnSearchAsync` → `Module.InitializeDataAsync` 経由。

**原因:** モジュールの `SearchLayouts` が `{}` (空辞書) のときに発生する。CLB ランタイムは `GetSearchCondition()` で `moduleDesign.SearchLayouts.TryGetValue(layoutName, out var sl)` し、見つからない場合 **空の `SearchCondition` (`ModuleName: ""`)** を返す。この後 `ListField.SetAdditionalConditionAsync` で `condition.ModuleName != ModuleName` で例外。

**対策:** `SearchLayouts` に必ず **`""` キーで空の SearchGridLayoutDesign** を入れる。検索画面が要らないモジュールでも、最低限の空レイアウトを置く。

```json
"SearchLayouts": {
  "": {
    "OnSearchInitialization": "",
    "ShowDefaultSearchButtons": true,
    "Layout": {
      "Operator": "And",
      "Name": "",
      "Padding": {},
      "IsBordered": false,
      "IsFlowLayout": false,
      "IsAutoFillWrap": false,
      "IsFillAvailable": false,
      "ScrollDirection": "Unset",
      "BackgroundColor": "",
      "Rows": [],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
    }
  }
}
```

**影響範囲:**
- 一覧ページ (ListPage = `ModulePageType: Auto`) で開かれる全モジュール
- ダイアログ用や表示専用モジュールでも、デザイナで Listボタンから開かれる可能性があるので**入れておく方が安全**

詳細は `Source/Codeer.LowCode.Blazor/OperatingModel/SearchField.cs:141 GetSearchCondition` および `ListField.cs:236` を参照。

---

## 40. 親子 ListField の絞り込みは `SearchCondition.Condition` で書く (LinkFieldNames だけでは効かない)

**症状:** 親モジュールの詳細画面で子モジュールの ListField を表示すると、**親のレコードと無関係な子レコードまで全件表示** されてしまう。例: `Order` 詳細を開いたら全 OrderDetail (他の注文のものまで) が一覧に出る。

**原因:** `LinkFieldNames` は他モジュールフィールドを「画面上で参照可能にする」設定で、**SearchCondition の自動絞り込みは別途必要**。子 ListField の `SearchCondition.Condition` に `FieldVariableMatchCondition` を入れて、**子側の外部キー = 親側の Id** を明示しなければならない。

**正しい設定:**

```json
{
  "Name": "Details",
  "SearchCondition": {
    "LimitCount": 50,
    "ModuleName": "OrderDetail",
    "Condition": {
      "IsOrMatch": false, "IsNot": false,
      "Children": [
        {
          "FieldName": "", "IsOrMatch": false, "IsNot": false,
          "Children": [
            {
              "SearchTargetVariable": "OrderId.Value",  // 子側の FK
              "Comparison": "Equal",
              "Variable": "Id.Value",                     // 親側の主キー
              "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition"
            }
          ],
          "Name": "",
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.FieldMatchCondition"
        }
      ],
      "Name": "",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition"
    }
  },
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
}
```

**ポイント:**
- `FieldVariableMatchCondition` の `SearchTargetVariable` は **子モジュール側** の外部キーフィールド名 (例: `OrderId.Value`)
- `Variable` は **親モジュール側** の主キー (通常 `Id.Value`)
- `FieldMatchCondition` で1段ラップする必要あり (Children の中に Children)

**多対多も同じパターン:** 中間モジュール (例: `ArticleTag`) を作って、親 (Article) の ListField に `ArticleId.Value = Id.Value` の絞り込みを入れる。中間モジュール内では `LinkField` で他方のマスタ (Tag) を参照。

参考: `Source/TestData/GettingStartedTemplate/App/Modules/Recipe.mod.json` の `IngredientList` 設定を参照。

---

## 41. 論理削除は Field 名を `LogicalDelete` にする (システム予約名)

**症状:** 削除ボタンを押すと物理削除されて履歴が消える。論理削除フラグを立てる動作にしたい。

**原因:** 単に `IsDeleted` のような任意のBoolean フィールドを作っても、CLB は物理削除を実行する。CLB の `SystemFieldNames.LogicalDelete` (= `"LogicalDelete"`) という**予約名**の Boolean フィールドが存在する場合に限り、`ModuleDataIOForDb` が `DELETE` を `UPDATE SET LogicalDelete = true` に置き換える。

**正しい設定:**

```json
{
  "UIType": "CheckBox",
  "DbColumn": "logical_delete",
  "DisplayName": "削除済み",
  "Name": "LogicalDelete",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
}
```

**自動動作:**
- 削除ボタン → `UPDATE table SET logical_delete = 1 WHERE Id = ?`
- 一覧表示で `LogicalDelete = false` のみフィルタ (CLB のデフォルト挙動)

**他のシステム予約名 (`SystemFieldNames.cs`):**
- `Id` - 主キー
- `LogicalDelete` - 論理削除フラグ (Boolean)
- `CreatedAt` / `UpdatedAt` - 作成・更新時刻 (自動)
- `Creator` / `Updater` - 作成者・更新者 (要認証)
- `OptimisticLocking` - 楽観ロック (= OptimisticLockingFieldDesign)
- `CurrentUser` - スクリプトで現在ユーザー参照

詳細は `Source/Codeer.LowCode.Blazor/DesignLogic/SystemFieldNames.cs` 参照。

**論理削除フラグを管理画面で見たい場合の回避策:**

`LogicalDelete` 予約名で動くモジュールは、フラグが立った行が自動で非表示になるので、削除済データを確認・復活させる管理画面が作れない。**別モジュールを同じ DbTable に対して定義**し、フラグの Field 名を **`LogicalDelete` 以外** (例: `DeletedFlag`) にすることで、CLB の自動論理削除フィルタを回避できる。

```json
// 管理用モジュール (SoftDeleteItemAdmin.mod.json)
{
  "Name": "SoftDeleteItemAdmin",
  "DataSourceName": "PatternsSQLite",
  "DbTable": "SoftDeleteItems",  // 同じテーブル
  "Fields": [
    { "Name": "Id", "TypeFullName": "...IdFieldDesign" },
    { "Name": "Name", "DbColumn": "Name", "TypeFullName": "...TextFieldDesign" },
    {
      "UIType": "CheckBox",
      "DbColumn": "logical_delete",      // 同じ DB 列
      "Name": "DeletedFlag",              // ← LogicalDelete 以外の名前
      "DisplayName": "削除フラグ",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign"
    }
  ]
}
```

これで:
- 元モジュール (`LogicalDelete` 命名) → ユーザー画面: 削除済み行は非表示、削除ボタンは論理削除動作
- 管理モジュール (`DeletedFlag` 等の命名) → 全件表示、フラグ ON/OFF 編集可能

**重要 — `LogicalDelete` フィールドは UI に出さない (常識):**

`LogicalDelete` はシステムフラグなので **ListLayout の Elements / DetailLayout の Rows のどちらにも入れない**。Fields の定義だけあれば論理削除動作は自動で効く。ユーザーが直接フラグを触る画面が必要なら、上の「管理モジュール」を別途用意する。

**管理用モジュールの推奨設定:**
- `CanDelete: false` (物理削除されると履歴が消えるため、誤操作を防止)
- `CanCreate: false` (管理画面では新規作成しない、元モジュールから入れる)
- ListPage の `CanUpdate: false` + `CanNavigateToDetail: true` (詳細画面で編集して復活させる)
- PageFrame Link の `UseNavigateToCreate: false`、`ListFieldDesign.CanCreate/Update/Delete: false`

---

## 42. 楽観ロックは Field 名を `OptimisticLocking` にする + `IncrementVersion: true` (絶対常識)

**症状:** OptimisticLockingFieldDesign を置いても CLB の自動楽観ロックが効かない (更新時の競合検出が動かない、Version カラムが自動更新されない)。

**原因 (両方が必須条件):**
1. **`Name: "OptimisticLocking"` (システム予約名)** ― `SystemFieldNames.OptimisticLocking` で CLB がフィールドを認識する。Name が "Version" など別名だと、`ModuleDataIOForDb` がフィールドを見つけられず、SELECT に Version カラムが含まれず、UPDATE WHERE 句にも入らない (= 楽観ロックが効かない)
2. **`IncrementVersion: true`** ― デフォルト `false` だと PostgreSQL の `xmin` のような DB 側採番機構前提。SQLite/SQL Server/MySQL でアプリ側のバージョン管理が必要なら true を明示

**正しい設定:**
```json
{
  "DbColumn": "Version",                  // DB の列名は何でもよい
  "IncrementVersion": true,                // 自動インクリメント
  "Name": "OptimisticLocking",             // ← Field 名は必ず予約名
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.OptimisticLockingFieldDesign"
}
```

**動作:** 更新時に `UPDATE ... SET Version = Version + 1 WHERE Id = ? AND Version = ?` の WHERE 句でバージョン照合し、影響行数 0 なら競合検出。

**UI には出さない:** `OptimisticLocking` はシステム項目なので、ListLayout / DetailLayout のどちらにも入れない (`LogicalDelete` と同じ)。Fields 定義だけあれば CLB が自動で扱う。

**PostgreSQL 例外:** `xmin` カラム使用時は `IncrementVersion: false` のまま (DB が自動更新)。詳細は `Source/App/Designer.WpfApp/App.xaml.cs` の `DbColumnTransformHandler` で `xmin` 自動マッピングを参照。

---

## 42-A. CLB のシステムフィールド一覧 (`SystemFieldNames.cs` で予約名定義)

**Field の `Name` プロパティを以下の予約名にすると、CLB ランタイムが自動で特別な振る舞いを行う**。これら「システムフィールド」を知らずに任意の名前を付けると、本来意図した自動動作 (論理削除フィルタ・楽観ロック・遷移URL組立て等) が一切効かない。

| 予約名 | フィールド型 | 自動動作 | UI 表示 |
|---|---|---|---|
| `Id` | `IdFieldDesign` | 主キー。一覧→詳細遷移 URL `/Module/{id}` を組み立てる。`ListField` の親子フィルタ (`Variable: "Id.Value"`) に必須 | **❌ 非表示** ([#45](#45-id-連番システム-id-は表示しないただし-id-定義は必須-絶対常識)) |
| `LogicalDelete` | `BooleanFieldDesign` | 削除ボタンを `UPDATE SET logical_delete = 1` に置換 + 一覧から自動除外 | **❌ 非表示** ([#41](#41-論理削除は-field-名を-logicaldelete-にする-システム予約名)) |
| `OptimisticLocking` | `OptimisticLockingFieldDesign` | 更新時にバージョン照合で競合検出 (`IncrementVersion: true` 併用) | **❌ 非表示** |
| `CreatedAt` | `DateTimeFieldDesign` | 新規作成時に現在時刻を自動セット | **任意** (見せても可) |
| `UpdatedAt` | `DateTimeFieldDesign` | 更新時に現在時刻を自動セット | **任意** (見せても可) |
| `Creator` | `TextFieldDesign` (推奨) | 新規作成時にログインユーザーを自動セット (要認証) | **任意** |
| `Updater` | `TextFieldDesign` (推奨) | 更新時にログインユーザーを自動セット (要認証) | **任意** |
| `CurrentUser` | (Field じゃない) | スクリプトで `CurrentUser` 識別子で現在ユーザーモジュール参照 | — |

**ルール (重要):**
- これらの予約名は **大文字小文字含めて厳格にこの綴り**。`Id`, `LogicalDelete`, `OptimisticLocking`, ... のキャメルケース
- 別の名前 (例: `IsDeleted`, `Version`, `RecordId`) では **自動動作が一切効かない**
- `Name` がシステム予約名、`DbColumn` は任意 (DB スキーマに合わせる)
- Id 以外のシステムフィールド (`LogicalDelete`/`OptimisticLocking`) は **基本 UI 非表示**。ユーザーが触る必要がない裏側のフラグ
- フラグを見たい/編集したい管理画面が必要な場合は、**別モジュールで Field 名を変える** (例: `LogicalDelete` → `DeletedFlag`)。これで CLB の自動動作を回避できる ([#41](#41-論理削除は-field-名を-logicaldelete-にする-システム予約名) の「管理用モジュールの推奨設定」)

詳細は `Source/Codeer.LowCode.Blazor/DesignLogic/SystemFieldNames.cs` 参照。

---

## 43. CSV/Excel インポート・エクスポートは PageFrame の Link で `CanBulkDataUpdate/Download` を立てる

**症状:** 一覧画面に CSV のインポート・エクスポートボタンが出ない。

**原因:** `CanBulkDataUpdate` / `CanBulkDataDownload` は **PageFrame の Link.ListPageDesign** に立てる必要がある。モジュール側 (`*.mod.json` の同名プロパティ) ではなく、リンク側で有効化する。

```json
"ListPageDesign": {
  "CanBulkDataUpdate": true,    // CSV/Excel インポート (右上にアップロードアイコン)
  "CanBulkDataDownload": true,  // CSV/Excel エクスポート (右上にダウンロードアイコン)
  ...
}
```

モジュール側にも同名フラグがあるが、ここは「機能をモジュールが許可するかどうか」のフラグ。両方 true にすれば一覧画面に CSV ボタンが出る。

---

## 43-A. SearchGridLayoutDesign.Operator で AND/OR 切替・ユーザー指定・ネスト

**ポイント:**
- `SearchGridLayoutDesign.Operator` は `"And" / "Or" / "UserSpecified"`
- **`"UserSpecified"`** にすると **画面上に AND/OR ドロップダウン**が表示され、ユーザーが実行時に切替可能 (デザイン時はデフォルト AND)
- `SearchGridLayoutDesign` を**入れ子**にすると `((A op B) op (C op D))` のような多階層論理式を作れる。内側を `IsBordered: true` にすると境界が見える

```json
"SearchLayouts": {
  "": {
    "Layout": {
      "Operator": "UserSpecified",
      "Rows": [
        { "Columns": [{ "Layout": {
          "Operator": "UserSpecified",
          "IsBordered": true,
          "Rows": [ /* group A: TagA, TagB */ ],
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
        }, ... }] },
        { "Columns": [{ "Layout": {
          "Operator": "UserSpecified",
          "IsBordered": true,
          "Rows": [ /* group B (further nesting possible) */ ],
          "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
        }, ... }] }
      ],
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
    }
  }
}
```

**TextField の比較演算子も画面で選べる:** `SearchComparisonDefaultValue: "Like"` / `"Equal"` を Field 定義に書くと、画面の各テキスト欄に **比較演算子ドロップダウン** が出る (ユーザーが Like/Equal を切替可能)。

**NumberField / DateField / DateTimeField の範囲検索は標準機能:** 1 フィールド置くだけで **`From 〜 To` の範囲入力 UI が自動で出る**。**`PriceFrom`/`PriceTo` のような補助フィールドを別途作るのは間違い。** NumberField/DateField の実装 (`Source/Codeer.LowCode.Blazor/OperatingModel/NumberField.cs` 等) を読むと、Search モード時に Min/Max の入力が両方標準で出る。

---

## 44. 一覧ページで「行選択+一括削除/更新」はできない — 詳細レイアウト内 ListField で実装する

**症状:** 一覧画面 (ListPage) で各行にチェックボックスを置いて、選択した行をボタンで一括削除/更新したい。`CanSelect: true` で行選択UIは出るが、それは「ハイライト」だけで一括処理ボタンとセットになっていない。

**理由:** ListPage は CLB 内部で動的生成される `$List$ModuleName` モジュールに `ListPageList` / `ListPageSearch` / `ListPageSubmit` のシステムフィールドが入る構造で、**任意のボタンを追加できない**。一括処理 UI を作る場所がない。

**正しい構造:**

「親モジュール (表示専用)」を作り、その **詳細レイアウト** に:
1. 子モジュールを参照する `ListFieldDesign` (Items 列)
2. 子モジュールの `ListLayout.Elements` に **`BooleanFieldDesign` の IsSelected 列** を入れる (チェックボックス表示)
3. 親モジュールに「選択行を削除」ボタン + スクリプト
4. 親モジュールに `SubmitButtonField` (DB 反映)
5. PageLink を **`ModulePageType: "Detail"`** にして、URL `/Main/親モジュール名` で直接詳細を開く

**親モジュール (BulkProduct.mod.json) のフィールド:**
```json
"Fields": [
  { "BackButton ..." },
  { "PageTitle (Label) ..." },
  { "Description (Label) ..." },
  {
    "ModuleName": "BulkProductItem",
    "SearchCondition": { "ModuleName": "BulkProductItem", "LimitCount": 50, ... },
    "CanCreate": true, "CanUpdate": true, "CanDelete": true,
    "Name": "Items",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign"
  },
  {
    "Text": "選択行を削除", "Variant": "Danger",
    "OnClick": "DeleteSelected_OnClick",
    "Name": "DeleteSelected",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.ButtonFieldDesign"
  },
  { "SubmitButton ..." }
]
```

**スクリプト (BulkProduct.mod.cs):**
```csharp
void DeleteSelected_OnClick()
{
    foreach (var row in Items.Rows)
    {
        if (row.IsSelected.Value == true)
        {
            Items.DeleteRow(row);
        }
    }
}
```

**子モジュール (BulkProductItem.mod.json) の ListLayout.Elements:**
```json
"Elements": [[
  { "FieldName": "IsSelected", "Width": 60 },
  { "FieldName": "Name", "Width": 320 },
  { "FieldName": "Price", "Width": 160 }
]]
```

`IsSelected` は **`DbColumn` を持たない BooleanField** (UI のみのフラグ、DB には保存しない)。

**PageLink (Main.frm.json):**
```json
{
  "Title": "一括処理",
  "ModulePageType": "Detail",
  "Module": "BulkProduct"
}
```

`Detail` 指定で URL `/Main/BulkProduct` 直接開く動作になる。

**ポイント:**
- 親モジュールは表示専用 (`DbTable: ""`, `DataSourceName: ""`) だが、入力フィールド (ListField のチェックボックス含む) があるので `CanUpdate: true` 必須 ([#27](#27-表示専用モジュールでも入力があるなら-canupdate-true)参照)
- 子モジュール (BulkProductItem) は **OtherPageModuleDesigns にも登録** ([#38](#38-pageframe-に登録されてないモジュールは画面が真っ白になる)参照)
- `ListField.DeleteRow(row)` で内部リストから行を消し、`SubmitButton` で物理削除が DB に反映される
- 同じ流れで「選択行に共通の値を一括セット」も書ける (`row.Name.Value = ...`)

詳細は CLB のソース `Components/AppParts/Page/ListPageComponentVM.cs` と `Repository/Design/PageLink.cs` を参照。

---

## 45. Id (連番システムフィールド) は表示しない、ただし Id 定義がないと詳細画面に進めない (絶対常識)

**ルール (両立):**
1. **`Id` フィールド (連番システム ID) は ListLayout/DetailLayout のどちらにも表示しない** — よほどの理由 (検索キー入力など) がない限り出さない
2. **DB と繋がっているモジュール (DbTable / DataSourceName を持つもの) は `Id` (`IdFieldDesign`) のフィールド定義が必須** — Fields 配列に定義がないと CLB が「一覧画面から詳細画面に遷移する」動線を組まない (詳細遷移ボタンが出ない、URL `/Module/{id}` が作れない、`ListField.SearchCondition.Variable: "Id.Value"` も解決できない)
   - **DB と繋がらない表示専用モジュール** (`DbTable: ""` / `DataSourceName: ""`、ダイアログ・ダッシュボード等) は **Id 定義不要**。「どのデータを表示するか」の特定が要らないため

**症状 (1 違反):**
- ListLayout の Elements に `{"FieldName": "Id"}` を入れると、**左端に空セル**が出る (IdField は UI に値を描画しないため)
- DetailLayout の Rows に Id を出すと数値カラムが見栄え悪く出る

**症状 (2 違反):**
- 表示専用モジュールっぽいから Id 定義を消す → 一覧から詳細へ遷移できない (`CanNavigateToDetail: true` でも `>` ボタンが押せない、`/Main/Module/{id}` URL が組まれない)
- `ListField` の親子フィルタが組めない (`Variable: "Id.Value"` が解決できない)

**正しいパターン:**

```json
{
  "Name": "Product",
  "DbTable": "Products",
  "Fields": [
    // Id 定義は必須 (UI 出さなくても入れる)
    {
      "DbColumn": "Id",
      "IsManualInput": false,
      "Name": "Id",
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign"
    },
    { "Name": "Name", "DbColumn": "Name", "TypeFullName": "...TextFieldDesign" },
    // ...
  ],
  "ListLayouts": {
    "": {
      "Elements": [[
        // Id は入れない
        { "FieldName": "Name", "Width": 240 },
        { "FieldName": "Price", "Width": 120 }
      ]]
    }
  },
  "DetailLayouts": {
    "": {
      "Layout": {
        "Rows": [
          // Id 表示行は入れない (新規作成時は空、更新時は連番が出るだけで意味なし)
          { "Columns": [{ "Layout": { "FieldName": "Name", ... } }] },
          // ...
        ]
      }
    }
  }
}
```

**例外 (Id を表示しても良いケース):**
- 業務 ID (受注番号 SO-0001 等) — ただしこの場合は `IsManualInput: true` の独自 IdField か、`TextField` を別途用意して扱うほうが筋がいい
- 開発デバッグ画面で連番を見たいとき
- 管理者向けの内部画面

**ListNumberField を使う:**
行番号を見せたい場合は `IdField` ではなく `ListNumberFieldDesign` を Fields に追加し、ListLayout の先頭列に入れる。GettingStarted の `Author.mod.json` / `Recipe.mod.json` がこのパターン。

```json
"Fields": [
  { "Name": "Id", "TypeFullName": "...IdFieldDesign" },
  { "Name": "ListNumber", "TypeFullName": "...ListNumberFieldDesign" },
  ...
],
"ListLayouts": {
  "": { "Elements": [[
    { "FieldName": "ListNumber", "Width": 60 },
    { "FieldName": "Name", "Width": 240 }
  ]] }
}
```

詳細は `Source/Codeer.LowCode.Blazor/Repository/Design/IdFieldDesign.cs` と `ListNumberFieldDesign.cs` を参照。

---

## 46. 検索ページのスクリプトで `Value` をセットしても効かない (絶対常識)

**症状:** `OnSearchInitialization` 等の検索コンテキストのスクリプトで `Status.Value = "進行中"` のように書いても、検索条件に反映されず無視される。

**原因:** 検索ページのフィールドは **検索専用プロパティ** で動いている。Detail/List ページ用の `.Value` とは別系統。CLB のソース (`Source/Codeer.LowCode.Blazor/OperatingModel/*Field.cs`) を見ると、各 Field クラスが `[ScriptMethodToProperty("SearchValue")]` 属性で SetSearchValueAsync を公開している。これが検索条件のセッター。

**Field 種類別の検索プロパティ一覧:**

| Field 種類 | 検索プロパティ | スクリプト例 |
|---|---|---|
| `TextField` / `IdField` / `LinkField` | `SearchValue` (string?) + `SearchComparison` | `Name.SearchValue = "進行中";` |
| `BooleanField` | `SearchValue` (bool?) | `IsActive.SearchValue = true;` |
| `SelectField` / `RadioGroupField` | `SearchValue` (単一, string?) / `SearchValues` (複数, List<string?>) | `Status.SearchValue = "InProgress";` |
| `NumberField` / `DateField` / `DateTimeField` / `TimeField` (= `RangeSearchField`) | `SearchMin` / `SearchMax` | `Price.SearchMin = 100; Price.SearchMax = 1000;` |

**正しい例 (`OnSearchInitialization` で初期検索条件をセット):**

```csharp
void InitSearch()
{
    // 単一値: Select / Boolean / Text / Link は SearchValue
    Status.SearchValue = "InProgress";
    IsActive.SearchValue = true;

    // 範囲: Number / Date / DateTime / Time は SearchMin / SearchMax
    Price.SearchMin = 100;
    Price.SearchMax = 1000;
    DueDate.SearchMin = DateOnly.FromDateTime(DateTime.Today);
    DueDate.SearchMax = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
}
```

**❌ 間違い:**
```csharp
void InitSearch()
{
    Status.Value = "InProgress";       // ❌ 検索条件に反映されない
    Price.Value = 100;                  // ❌ そもそも検索ページに Value プロパティはない
}
```

**OnSearchInitialization の発火条件:**

`SearchLayoutDesign.OnSearchInitialization` に書いたスクリプトは、URL に `?initialize_search=true` がついている時だけ発火する。
- サイドバー Link から飛んだ場合は CLB が自動でこのパラメータを付ける (`OnSearchInitialization` が空でない場合のみ)
- 直接 URL を打ち込んだ場合は付かない → スクリプトは発火しない
- そのため Playwright でテストするときも `https://localhost:7169/Main/MyModule?initialize_search=true` で叩く

詳細は `Source/Codeer.LowCode.Blazor/OperatingModel/SearchField.cs` の `InitializeDataAsync` と `RemoveInitializeSearch` を参照。

## 47. PageFrame の Link / Module 等の複雑 JSON を Python で「ゼロから書き起こさない」 (致命的)

**症状:** Python スクリプトで `Link` を辞書リテラルから組み立てて `PageFrame.frm.json` に書き戻したら、**サイドバーが完全に消失して画面が真っ白**。ナビゲーション全滅で、サーバを再起動するまで何も操作できなくなる。

**原因:** PageFrame の `Link` は `ListPageDesign` / `DetailPageDesign` / `ListFieldDesign` / `SearchCondition` などが何段にも入れ子になっており、各レベルに必須プロパティが多数ある。デフォルト値が C# 側の `= ` 初期化に依存しているプロパティ (`IsStriped`, `IconType`, `HideTitle`, `CanCustomizeColumns`, `ApplyBackgroundToBoxInput`, `OnDoubleClickRow`, `OnValidateInput` 等) を **1 つでも欠落させると、JSON デシリアライズが NRE で失敗 → PageFrame 自体が読み込めない → サイドバー全滅** という連鎖が起きる。

似た構造の `OtherPageModuleDesigns` も同じ。Module / Field / Layout も同様で、安全圏のキー網羅は人間が覚えで書けるレベルではない。

**対策（必ずこれを徹底）:**

```python
import copy
import json
from pathlib import Path

page = json.loads(Path("App/PageFrames/Main.frm.json").read_text(encoding="utf-8"))

# 既存 Link を 1 件取り出して deepcopy → 必要箇所だけ書き換え
template = next(l for l in page["Left"]["Links"] if l["Title"] == "商品一覧")
new_link = copy.deepcopy(template)
new_link["Title"] = "ダイアログ/編集"
new_link["Module"] = "EditDialogSample"
new_link["ListPageDesign"]["ListFieldDesign"]["LayoutName"] = ""  # 必要なら
page["Left"]["Links"].append(new_link)

Path("App/PageFrames/Main.frm.json").write_text(
    json.dumps(page, ensure_ascii=False, indent=2), encoding="utf-8"
)
```

- 既存 Link が 1 件もない初期状態なら、**先に Designer で 1 件作って commit してから** スクリプト処理に入る。
- スクリプト中で空 dict から組み立てるパターン (`{"Title": ..., "ListPageDesign": {...}}`) は **絶対に書かない**。
- `Module.mod.json` / `*FieldDesign` / `*LayoutDesign` も同じ。新規生成時は既存の同じ型のファイル/フィールド/レイアウトを deepcopy するのが安全。
- どうしてもゼロから組む必要がある場合は、組んだ JSON を Designer で開いて読み込めるか確認してから本番に書き込む。

**やってはいけない例:**
```python
# ❌ これで一発でサイドバーが消える
new_link = {
    "Title": "ダイアログ/編集",
    "Module": "EditDialogSample",
    "ListPageDesign": {
        "ListFieldDesign": {
            # IsStriped / OnDoubleClickRow / IgnoreModification 等が欠落 → NRE
            "LayoutName": "",
            "CanNavigateToDetail": True,
            ...
        }
    }
}
```

「動いてるように見える Link」を見つけたら、それが真の正解スキーマ。**スキーマを記憶や推測で書かない**。

## 48. FileField / ImageField はサーバ側設定 + 一時ファイルテーブルが必須

**症状:** デザインプロジェクトに `FileField` を置いて、画面ではファイル選択 UI が出るのに、アップロードがランタイム例外で失敗する / 保存ボタンを押しても何も起きない。

**原因:** `FileField` は **デザインプロジェクトの JSON だけでは動かない**。サーバ側 (`appsettings.json`) で一時ファイル管理テーブルの定義が必要で、対応する DB テーブルも事前に作っておく必要がある。

**対策 (3点セット):**

1. `appsettings.json` に **`TemporaryFileTableInfo`** を追加 (DataSource ごと):
   ```json
   "TemporaryFileTableInfo": [
     {
       "DataSourceName": "Main",
       "Table": "temporary_files",
       "GuidColumn": "guid",
       "CreatedDateTimeColumn": "created_date_time"
     }
   ]
   ```

2. 対応する DB テーブル作成 (各 DataSource):
   ```sql
   CREATE TABLE temporary_files (
     guid TEXT PRIMARY KEY,
     created_date_time DATETIME NOT NULL
   );
   ```
   列名/型は `appsettings.json` の指定に合わせる。

3. `designer.settings.json` に **`FileStorages`** を 1 つ以上定義し、`FileField.StorageName` でそれを参照:
   ```json
   "FileStorages": [{ "Name": "Local", "FileStorageType": "FileSystem" }]
   ```

これらが揃ってない環境でデモ用に `FileField` を追加すると、必ず失敗するので、サンプル/ショーケース系では「サーバ側設定が要るので別途準備が必要」と注記するか、もしくは入れない。

詳細は [Docs/Fields/FileField.md](Fields/FileField.md) の「サーバ側設定が必須」セクション参照。

## 49. `GridRow.KeepInFillAvailableGrid` を最終行で `true` にしてはいけない (超絶レア)

`KeepInFillAvailableGrid` プロパティはデフォルトの `false` のまま使うのが**ほぼすべての場合の正解**。`true` を立てるケースは超絶レアで、知らずに立てると `IsFillAvailable=true` の効果 (画面下端までリストが広がる) が消えて、見た目バグになる。

### 仕組み

`IsFillAvailable=true` の Grid の **最終行 (FillAvailable target)** には2つのモードがある:

| モード | KeepInFillAvailableGrid | 意味 | 適用対象 |
|---|---|---|---|
| **DirectList (デフォルト)** | `false` | 行 `height: Npx` 固定。中の要素は内部スクロールする想定 | `ListField` / `DetailListField` / `TileListField` / `ProCodeField` (自前スクロールあり) |
| **FitContent (超絶レア)** | `true` | 行 `min-height: Npx`。中身がコンテンツサイズで止まり、はみ出した分はページスクロール | `Button` / `Label` / 静的コンテンツの最終行 |

### 何が起きるか

- ListField を最終行に置いたとき、`KeepInFillAvailableGrid=false` (デフォルト) なら **ListField の `.list-scroll` が画面下端まで広がり、データが多ければ内部スクロール、少なくても枠は画面下端まで**。
- 同じ ListField で `KeepInFillAvailableGrid=true` にすると、**ListField のテーブルがコンテンツサイズで止まり、画面下端まで広がらない** (CSS的に `min-height: 100%` の card になるが、内部スクロールが効かない見た目に)。

### 立てて良いケース (超絶レア)

`IsFillAvailable=true` の Grid で、**実質最終行が ListField/ProCode じゃなく、Button や見出し Label のような固定高さの要素** のとき。例えば「上にスクロール可能なリスト、下にボタンバーを画面下端から押し出して見せたい (はみ出し時はページスクロール)」のような特殊レイアウト。`Source/TestData/UITest/Modules/LastRowFitButtonTest.mod.json` がその例。

> 「リストを最終行に置きたい」のような一般用途では **必ず `false` のまま** にする。

### よくある事故

「FillAvailable で List が画面下端まで広がる」と聞いて、List 行に `KeepInFillAvailableGrid=true` を立ててしまうのが典型ミス。プロパティ名に "FillAvailable" が入っているので「FillAvailable させる ON フラグ」と誤解しがちだが、意味は逆 (`false` がデフォルトで FillAvailable 動作、`true` はモード切替の特殊フラグ)。

詳細は [Docs/Layouts.md](Layouts.md) の `GridRow` プロパティ表参照。

## 50. 「ラベル列 + 入力列」 2カラムフォームのラベル列には `VerticalAlignment: "Middle"` をマスト設定

`<NameLabel(120px)> | <Name(伸縮)>` のようなラベル+入力2カラム行で、ラベル列の `Column.VerticalAlignment` を **必ず `"Middle"` にする**。

### なぜマストか

ラベル列を `Middle` にしないと、入力列が単行 Text のときは見た目が変わらないが、以下のケースで **ラベルだけがセル上端に張り付いて、入力欄の中央からずれる** 破綻が起きる:

- Multiline TextField (`IsMultiline=true`, `IsAutoFitRows=true`) で複数行になったとき
- FileField のプレビュー枠が大きく表示されたとき
- 入力欄の Placeholder が長くて自動折り返ししたとき
- ListField / DetailListField を入力欄に埋め込んだとき
- ラベル自体の文字数が多くて折り返したとき (ラベル幅 80px に「最低価格」みたいな4文字を入れた場合等)

短文の Text Field だけで構成された画面なら気づかないが、**後で必ず破綻する**。最初から全部 `Middle` にしておくのが正解。

### 設定例 (JSON)

```json
{
  "IsWrap": false,
  "Columns": [
    {
      "Layout": { "FieldName": "NameLabel", "TypeFullName": "...FieldLayoutDesign" },
      "Width": 120,
      "VerticalAlignment": "Middle"
    },
    {
      "Layout": { "FieldName": "Name", "TypeFullName": "...FieldLayoutDesign" },
      "Width": 320
    }
  ]
}
```

### 自動修正

既存ファイルでラベル列の `VerticalAlignment` が抜けている場合、以下のような Python 一括修正で安全に当てられる (FieldName が `xxxLabel` で終わり、同 Row に非 Label Field が同居する Column が対象):

```python
for r in rows:
    cols = r.get("Columns", [])
    fns = [(c.get("Layout") or {}).get("FieldName", "") for c in cols]
    if len(cols) >= 2:
        for i, fn in enumerate(fns):
            others = [n for j, n in enumerate(fns) if j != i and n and not n.endswith("Label")]
            if fn.endswith("Label") and fn != "Label" and others:
                cols[i]["VerticalAlignment"] = "Middle"
```

> ラベル+入力レイアウトを **既存サンプル (`FormLayoutSample` / `EditDialogTarget` / `ShowPanelTarget` 等) から deepcopy** すれば、最初からこの設定が入っているので考えずに済む。新規にゼロから組むときだけ注意。


## 51. `SearchTargetVariable` / `Variable` は Variable (`Id.Value`) であって Field (`Id`) ではない (致命的)

### 何を間違えるか

SearchCondition の `FieldValueMatchCondition` / `FieldVariableMatchCondition` の `SearchTargetVariable` および `Variable` プロパティに、**Field 名だけを書く**間違いが多い:

```json
// ❌ 間違い: "Id" は Field 名
{
  "SearchTargetVariable": "Id",
  "Comparison": "Equal",
  "Value": { "Value": "123", "TypeFullName": "...StringValue" },
  "TypeFullName": "...FieldValueMatchCondition"
}
```

```json
// ✅ 正解: Variable は `Field名.メンバ名` (`Id.Value`)
{
  "SearchTargetVariable": "Id.Value",
  "Comparison": "Equal",
  "Value": { "Value": "123", "TypeFullName": "...StringValue" },
  "TypeFullName": "...FieldValueMatchCondition"
}
```

### 結果

`Id フィールドが存在しません` 等の例外で SQL 生成が失敗する (`SelectSqlCreator.CreateWhereValueCondition` 内で投げられる)。

### CLB における Field と Variable の違い

- **Field 名**: 例 `Id`, `Status`, `Email` — モジュール内の Field の論理名
- **Variable**: 例 `Id.Value`, `Status.Value`, `LinkField.Email.Value` — 「実際に値が入っている場所」までを示すパス。**Field 名 + `.` + データクラスのメンバ名** で構成

各 Field 型の値メンバはほぼ `Value` だが、LinkField/SelectField の表示テキストは `DisplayText`、複合キーの個別キーも別、というように Field によって違う。**`.Value` まで書かないと意味が確定しない**。

### 主要 Field のデフォルト値メンバ

| Field 型 | 値メンバ | Variable 例 |
|---|---|---|
| TextField | `Value` | `Name.Value` |
| NumberField | `Value` | `Price.Value` |
| BooleanField | `Value` | `IsActive.Value` |
| DateField / DateTimeField / TimeField | `Value` | `CreatedAt.Value` |
| IdField | `Value` | `Id.Value` |
| LinkField | `Value` (FK 値) / `DisplayText` (表示) | `Category.Value` / `Category.DisplayText` |
| SelectField | `Value` (値) / `DisplayText` (表示) | `Status.Value` / `Status.DisplayText` |
| FileField | `FileGuid` / `FileName` / `FileSize` | `Attachment.FileName` |

リンク先のフィールドを参照する場合は `LinkField名.参照先Field名.Value` でドット繋ぎ (例: `Category.Name.Value`)。

### 設定が出てくる箇所 (デザイナでも JSON でも同じ)

- `FieldValueMatchCondition.SearchTargetVariable`
- `FieldVariableMatchCondition.SearchTargetVariable` / `Variable`
- `LinkFieldDesign.ValueVariable` / `DisplayTextVariable` ← ここも Variable
- `SelectFieldDesign.ValueVariable` / `DisplayTextVariable`
- `SortCondition.Variable`
- `SearchCondition.SelectFields` は逆に **Field 名** (`Id` / `Name` / `LinkField.SubField`) を指定 — こちらは Variable ではないので注意

ルール: **「変数 (Variable)」と書いてあるプロパティは `.Value` 等まで書く**、「フィールド (Field)」と書いてあるプロパティは Field 名のみ。
