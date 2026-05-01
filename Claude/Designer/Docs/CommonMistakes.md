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
