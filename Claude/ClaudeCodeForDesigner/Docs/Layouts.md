# レイアウト定義

モジュールの画面表示を制御するレイアウト定義のリファレンス。レイアウトには大きく分けて3種類のラッパー（DetailLayout, ListLayout, SearchLayout）と、その中で使用する配置レイアウト（GridLayout, TabLayout, CanvasLayout）がある。

> **このドキュメントの読み方 (Claude 向け)**
>
> 各クラスのセクションには **「C# クラス定義 (真実の源)」** を貼ってある。これはソースコード `Source/Codeer.LowCode.Blazor/Repository/Design/*.cs` から転記したもので、JSON のスキーマの **唯一の真実**。
>
> JSON で書く時に「このプロパティは配列か Dictionary か」「null 許容か」「初期値は何か」で迷ったら、必ず C# 定義を見ること。表形式の説明や JSON 例を読んで判断するのは曖昧さが残るため不可。
>
> 例: `public List<LayoutDesignBase> Layouts { get; set; }` なら **配列** (`[...]`)、`public Dictionary<string, LayoutDesignBase> Layouts` なら **オブジェクト** (`{"key": ...}`)。形が違えばデシリアライズで失敗してデザイナがモジュールを認識しなくなる。
>
> C# 定義の `[Designer(...)]` などの属性は無視して良い (デザイナー UI 用)。重要なのは:
> - **プロパティの型** (`List<T>` / `Dictionary<K,V>` / `T?` / `string` / 列挙型 など)
> - **初期値** (`= new()`, `= "";`, `= []`, デフォルト)
> - **継承関係** (`: LayoutDesignBase` なら親クラスのプロパティも持つ)
> - **`[Obsolete]` のついたプロパティは新規 JSON で使わない**

---

## 1. DetailLayoutDesign - 詳細画面レイアウト

フォーム/詳細画面のラッパー。モジュールの `DetailLayouts` ディクショナリに格納される。

### C# クラス定義 (真実の源)

```csharp
public class DetailLayoutDesign  // JsonAbstract 派生ではないので TypeFullName 不要
{
    public string OnBeforeInitialization { get; set; } = string.Empty;
    public string OnAfterInitialization { get; set; } = string.Empty;
    public string OnLocationChanging { get; set; } = string.Empty;     // bool 返却で離脱キャンセル
    public string OnFieldDataChanged { get; set; } = string.Empty;     // (string fieldName) 引数
    public List<string> DataOnlyFields { get; set; } = [];
    public string ClassName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public int? FontSize { get; set; }
    public LayoutDesignBase Layout { get; set; }   // 通常 GridLayoutDesign / TabLayoutDesign / CanvasLayoutDesign
        = new GridLayoutDesign { Rows = [GridRow.CreateEmptyRow()] };
}
```

> モジュールの `DetailLayouts` プロパティは `Dictionary<string, DetailLayoutDesign>` 型 (= JSON では **オブジェクト** `{"": {...}}`)。`""` (空文字列) キーがデフォルトレイアウト。

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `OnBeforeInitialization` | string | `""` | 初期化前に呼ばれるスクリプトイベント名 |
| `OnAfterInitialization` | string | `""` | 初期化後に呼ばれるスクリプトイベント名 |
| `OnLocationChanging` | string | `""` | ページ離脱前イベント。`false` 返却でキャンセル可能。 |
| `OnFieldDataChanged` | string | `""` | いずれかのフィールドの値が変更された時のイベント名。引数に変更されたフィールド名が渡される。 |
| `DataOnlyFields` | List\<string\> | `[]` | データとしてロードするが画面に表示しないフィールド名リスト |
| `ClassName` | string | `""` | CSSクラス名 |
| `Color` | string | `""` | 文字色（CSS色値） |
| `BackgroundColor` | string | `""` | 背景色（CSS色値） |
| `Layout` | LayoutDesignBase | | 内部レイアウト。通常は GridLayoutDesign。 |

### スクリプトイベントハンドラ

#### OnBeforeInitialization

画面の初期化前に呼ばれる。データロード前の設定に使用。

```csharp
void Detail_OnBeforeInit()
{
    // フィールドの初期制御（データロード前）
    EditPanel.IsVisible = false;
}
```

#### OnAfterInitialization

画面の初期化後に呼ばれる。データロード完了後の処理に使用。

```csharp
void Detail_OnAfterInit()
{
    // 初期化後に候補データをロード
    var clientSearcher = new ModuleSearcher<Client>();
    SelectClient.SetAdditionalCondition(clientSearcher);
    SelectClient.ReloadCandidates();

    // 新規データの場合のデフォルト値設定
    if (this.IsNewData)
    {
        Status.Value = "Draft";
        CreatedDate.Value = DateOnly.FromDateTime(DateTime.Today);
    }
}
```

#### OnLocationChanging

ページ離脱前に呼ばれる。`false` を返すとページ遷移をキャンセルできる。

```csharp
bool Detail_OnLeaving()
{
    if (this.IsModified)
    {
        var result = MessageBox.Show("未保存の変更があります。離れますか？", "はい", "いいえ");
        return result == "はい";
    }
    return true;
}
```

#### OnFieldDataChanged

いずれかのフィールドが変更された時に呼ばれる。引数 `string fieldName` に変更されたフィールド名が渡される。

```csharp
void Detail_OnFieldChanged(string fieldName)
{
    if (fieldName == "Quantity" || fieldName == "UnitPrice")
    {
        Amount.Value = Quantity.Value * UnitPrice.Value;
    }
}
```

### JSON例

```json
"DetailLayouts": {
  "": {
    "OnBeforeInitialization": "Detail_OnInit",
    "OnAfterInitialization": "",
    "OnLocationChanging": "Detail_OnLeaving",
    "OnFieldDataChanged": "Detail_OnFieldChanged",
    "DataOnlyFields": ["HiddenCalcField"],
    "ClassName": "",
    "Color": "",
    "BackgroundColor": "",
    "Layout": {
      "Rows": [
        {
          "IsWrap": false,
          "GridRowType": "Normal",
          "Columns": [
            {
              "Layout": {
                "FieldName": "Name",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": null
            }
          ]
        }
      ],
      "IsBordered": false,
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
    }
  }
}
```

---

## 2. ListLayoutDesign - 一覧画面レイアウト

テーブル/一覧表示のラッパー。モジュールの `ListLayouts` ディクショナリに格納される。

**注意:** 表示専用モジュール（`DbTable` が空、チャートダッシュボードやダイアログ等）では `ListLayouts` の Elements にフィールドを入れないこと。フィールドを入れると一覧表示モジュールと認識されてしまう。

### C# クラス定義 (真実の源)

```csharp
public class ListLayoutDesign
{
    [Obsolete] public string HeaderTitle { get; set; } = string.Empty;
    public List<string> DataOnlyFields { get; set; } = new();
    public string OnBeforeInitialization { get; set; } = string.Empty;
    public string OnAfterInitialization { get; set; } = string.Empty;
    public string OnFieldDataChanged { get; set; } = string.Empty;     // (string fieldName) 引数
    public List<List<ListElement>> Elements { get; set; } = [[new()]];
    // ↑ Elements は List<List<...>>。外側=行 (通常 1 個)、内側=その行内の列。
    //   通常の 1 行ヘッダーなら [[col1, col2, col3]] と書く。
    //   [[col1], [col2]] と書くと 2 行ヘッダー扱いで縦に並ぶので注意。
}

public class ListElement : IFontAppearance
{
    public string FieldName { get; set; } = string.Empty;
    public string ContextMenu { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double? Width { get; set; }
    public int ColumnSpan { get; set; } = 1;          // int (1.0 のような小数 NG)
    public int RowSpan { get; set; } = 1;             // int
    public bool? IsViewOnly { get; set; }
    public TextWrap TextWrap { get; set; }            // enum: Unset / BreakAll / Ellipsis
    public bool CanResize { get; set; }
    public bool CanUserSort { get; set; } = true;
    public HorizontalAlignment? HeaderHorizontalAlignment { get; set; }
    public HorizontalAlignment? HorizontalAlignment { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public int? FontSize { get; set; }                // int? (14.0 NG、14 と書く)
    public CssFontWeight? FontWeight { get; set; }
    public CssFontStyle? FontStyle { get; set; }
    public string Color { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string DetailLayoutName { get; set; } = string.Empty;
    public string ListElementComponent { get; set; } = string.Empty;
}
```

> モジュールの `ListLayouts` プロパティは `Dictionary<string, ListLayoutDesign>` 型 (= JSON では **オブジェクト** `{"": {...}}`)。`""` キーがデフォルト。

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `OnBeforeInitialization` | string | `""` | 初期化前スクリプトイベント名 |
| `OnAfterInitialization` | string | `""` | 初期化後スクリプトイベント名 |
| `OnFieldDataChanged` | string | `""` | フィールド変更イベント名 |
| `DataOnlyFields` | List\<string\> | `[]` | データロードのみ（非表示）のフィールド名リスト |
| `HeaderTitle` | string | `""` | 一覧のヘッダータイトル |
| `Elements` | List\<List\<ListElement\>\> | `[]` | テーブル列定義。**外側のリストがヘッダー行**（通常は1行なので要素1つ）、**内側のリストがその行内の列**。複数行ヘッダーの場合のみ外側の要素が2以上になる。 |

### ListElement プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `FieldName` | string | `""` | 表示するフィールド名 |
| `Label` | string | `""` | 列ヘッダーのラベル |
| `Width` | double? | null | 列幅（px）。null で自動。 |
| `ColumnSpan` | int | 1 | 列結合数。**整数のみ（`1.0` は不可）** |
| `RowSpan` | int | 1 | 行結合数。**整数のみ（`1.0` は不可）** |
| `TextWrap` | TextWrap | `"Unset"` | テキスト折り返し: `Unset` / `BreakAll` / `Ellipsis` |
| `CanResize` | bool | `false` | ユーザーによる列幅変更を許可 |
| `CanUserSort` | bool | `true` | 列ヘッダークリックでのソートを許可 |
| `HorizontalAlignment` | HorizontalAlignment | | 水平配置: `Start` / `Center` / `End` / `Stretch` |
| `HeaderHorizontalAlignment` | HorizontalAlignment | | ヘッダーの水平配置 |
| `IsViewOnly` | bool | `false` | 読み取り専用で表示 |
| `ClassName` | string | `""` | CSSクラス名 |
| `Color` | string | `""` | 文字色 |
| `BackgroundColor` | string | `""` | 背景色 |
| `FontFamily` | string | `""` | フォントファミリー |
| `FontSize` | int? | null | フォントサイズ。**整数のみ（`14.0` は不可、`14` と書くこと）** |
| `FontWeight` | CssFontWeight? | null | フォント太さ |
| `FontStyle` | CssFontStyle? | null | フォントスタイル |
| `DetailLayoutName` | string | `""` | セルの中にこの名前の DetailLayout をまるごと描画する。指定時は `FieldName` を空にする (通常列と混在可)。セル用のコンパクトな DetailLayout を別途用意するのが定石。実装サンプル: PatternShowcase の `DetailInListSample` + `ListSampleItems` (`DetailLayouts["InListCell"]` / `ListLayouts["DetailInList"]`) |
| `ContextMenu` | string | `""` | コンテキストメニューの ContextMenuField 名 |
| `ListElementComponent` | string | `""` | カスタムコンポーネント名 |

### JSON例（通常の1行ヘッダー）

全列を1つの内側配列にまとめる。`Elements[0]` がヘッダー行で、その中に各列を並べる。

```json
"ListLayouts": {
  "": {
    "HeaderTitle": "商品一覧",
    "DataOnlyFields": [],
    "OnBeforeInitialization": "",
    "OnAfterInitialization": "",
    "OnFieldDataChanged": "",
    "Elements": [
      [
        {
          "FieldName": "Id",
          "Label": "ID",
          "Width": 100,
          "ColumnSpan": 1,
          "RowSpan": 1,
          "TextWrap": "Unset",
          "CanResize": true,
          "CanUserSort": true,
          "HorizontalAlignment": "Start",
          "ClassName": "",
          "Color": "",
          "BackgroundColor": "",
          "DetailLayoutName": ""
        },
        {
          "FieldName": "Name",
          "Label": "商品名",
          "ColumnSpan": 1,
          "CanResize": true,
          "CanUserSort": true
        },
        {
          "FieldName": "Price",
          "Label": "価格",
          "Width": 120,
          "ColumnSpan": 1,
          "CanResize": true,
          "CanUserSort": true,
          "HorizontalAlignment": "End"
        }
      ]
    ]
  }
}
```

> Elements の構造に関する注意点は [LayoutGuidelines.md](LayoutGuidelines.md) を参照。

### JSON例（複数行ヘッダー）

ヘッダーを複数行にする場合は外側の配列に行を追加し、`RowSpan` / `ColumnSpan` でセル結合を制御する。`FieldName: ""` はスパンされたセルのプレースホルダー。

```json
"Elements": [
  [
    { "FieldName": "Id", "Label": "ID", "RowSpan": 2 },
    { "FieldName": "Name", "Label": "名前" },
    { "FieldName": "Price", "Label": "価格" }
  ],
  [
    { "FieldName": "" },
    { "FieldName": "Category", "Label": "カテゴリ" },
    { "FieldName": "Stock", "Label": "在庫" }
  ]
]
```

---

## 3. SearchLayoutDesign - 検索画面レイアウト

検索フォームのラッパー。モジュールの `SearchLayouts` ディクショナリに格納される。

### C# クラス定義 (真実の源)

```csharp
public class SearchLayoutDesign
{
    public string OnSearchInitialization { get; set; } = string.Empty;
    public bool ShowDefaultSearchButtons { get; set; } = true;
    public LayoutDesignBase Layout { get; set; }   // 通常 SearchGridLayoutDesign
        = new SearchGridLayoutDesign { Rows = [GridRow.CreateEmptyRow()], IsExpandable = true, ... };
}

public class SearchGridLayoutDesign : GridLayoutDesign
{
    public SearchOperator Operator { get; set; } = SearchOperator.And;   // enum: And / Or / UserSpecified
    // 親 GridLayoutDesign の全プロパティを継承 (Rows / Padding / IsBordered / IsExpandable / etc)
}
```

> モジュールの `SearchLayouts` プロパティは `Dictionary<string, SearchLayoutDesign>` 型 (= JSON では **オブジェクト** `{"": {...}}`)。`""` キーがデフォルト。
> SearchGridLayoutDesign の `TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign` (GridLayoutDesign ではなく専用)。

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `OnSearchInitialization` | string | `""` | 検索初期化時のスクリプトイベント名 |
| `ShowDefaultSearchButtons` | bool | `true` | デフォルトの検索/クリアボタンを表示するか |
| `Layout` | SearchGridLayoutDesign | | 検索フォームのレイアウト。GridLayoutDesign を拡張した SearchGridLayoutDesign を使用。 |

### スクリプトイベントハンドラ

#### OnSearchInitialization

検索画面初期化時に呼ばれる。検索条件の初期値設定や候補データのロードに使用。

```csharp
void Search_OnInit()
{
    // 検索フォームの候補データをロード
    var categorySearcher = new ModuleSearcher<Category>();
    SearchCategory.SetAdditionalCondition(categorySearcher);
    SearchCategory.ReloadCandidates();
}
```

### SearchGridLayoutDesign

GridLayoutDesign を継承し、以下のプロパティを追加。

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Operator` | string | `"And"` | 検索条件の結合方式: `And` / `Or` / `UserSpecified` |

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign`

**Operator の値:**
- `"And"` — 配下の条件はすべて AND 結合 (デフォルト)
- `"Or"` — 配下の条件はすべて OR 結合
- `"UserSpecified"` — カード先頭に AND/OR セレクタが表示され、ユーザーが切替可能

**UserSpecified のレイアウト:**

`UserSpecified` のとき、カード (`card-body`) の先頭に AND/OR セレクタ用の `grid-row` / `grid-column` が挿入される。下の検索フィールド列と同じ `grid-row` / `grid-column` 構造で配置されるため左端が揃う。既定幅 200px。

```html
<div class="card-body grid-frame">
  <div class="grid-row" data-row="mb">                <!-- AND/OR セレクタ行 -->
    <div class="grid-column" data-system="search-condition-field">
      <select>...AND/OR...</select>
    </div>
  </div>
  <div class="grid-row" data-row="...">                <!-- フィールド行 -->
    <div class="grid-column">...フィールド...</div>
  </div>
</div>
```

幅の調整は app.css から可能 (詳細は [Docs/AppCss.md](AppCss.md) の「検索 AND/OR セレクタの幅」セクション参照)。`--search-condition-field-width` CSS 変数または `.grid-column[data-system="search-condition-field"]` セレクタで上書きする。

ネスト構造で `Operator: "UserSpecified"` を使うと、ネストカード内に AND/OR セレクタが表示され、複雑な論理式 (例: `A AND (B OR (C AND D))`) を画面から組み立てられる。

### JSON例

```json
"SearchLayouts": {
  "": {
    "OnSearchInitialization": "",
    "ShowDefaultSearchButtons": true,
    "Layout": {
      "Operator": "And",
      "Rows": [
        {
          "IsWrap": false,
          "GridRowType": "Normal",
          "Columns": [
            {
              "Layout": {
                "FieldName": "Name",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": null
            },
            {
              "Layout": {
                "FieldName": "Status",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              },
              "Width": null
            }
          ]
        }
      ],
      "IsBordered": true,
      "IsExpandable": true,
      "ExpanderLabel": "検索条件",
      "IsExpanderDefaultOpened": false,
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SearchGridLayoutDesign"
    }
  }
}
```

---

## 4. GridLayoutDesign - グリッドレイアウト

最も基本的な配置レイアウト。行と列のグリッド構造でフィールドを配置する。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign`

### C# クラス定義 (真実の源)

```csharp
public abstract class LayoutDesignBase : JsonAbstract
{
    public virtual string Name { get; set; } = string.Empty;
    public bool? IsViewOnly { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
}

public class GridLayoutDesign : LayoutDesignBase
{
    public override string Name { get; set; } = string.Empty;
    public ThicknessDesign Padding { get; set; } = new();
    public bool IsBordered { get; set; }
    public bool UseBorderedShrinkWrap { get; set; }
    public bool IsExpandable { get; set; }
    public string ExpanderLabel { get; set; } = string.Empty;
    public bool IsExpanderDefaultOpened { get; set; }
    public bool IsFlowLayout { get; set; }
    public bool IsAutoFillWrap { get; set; }
    public bool IsFillAvailable { get; set; }
    public ScrollDirection ScrollDirection { get; set; }  // enum: Unset / Vertical / Horizontal
    public string OnKeyDown { get; set; } = string.Empty;
    public List<GridRow> Rows { get; set; } = new();
}

public class GridRow
{
    public bool IsWrap { get; set; }
    public bool IsAutoFillWrap { get; set; }
    public double? Height { get; set; }
    public ThicknessDesign Margin { get; set; } = new();
    public GridRowType GridRowType { get; set; }  // enum: Normal / Header / Footer
    public bool CanResize { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
    public bool IsProportionalScale { get; set; }  // 列幅を比率で拡大縮小
    [Obsolete] public bool IsRowMarginRemoved { get; set; }
    public virtual List<GridColumn> Columns { get; init; } = new();
}

public class GridColumn
{
    public LayoutDesignBase? Layout { get; set; }  // null=空セル / Field/Grid/Tab/Canvas/Flow
    public double? Width { get; set; }
    public double? MinWidth { get; set; }
    public double? MaxWidth { get; set; }
    public ThicknessDesign Padding { get; set; } = new();
    public string BackgroundColor { get; set; } = string.Empty;
    public BorderStyleDesign BorderStyle { get; set; } = new();
    public HorizontalAlignment? HorizontalAlignment { get; set; }  // enum: Start / Center / End / Stretch
    public VerticalAlignment? VerticalAlignment { get; set; }      // enum: Top / Middle / Bottom / Stretch
    public bool CanResize { get; set; }
    [Obsolete] public string? Background { get; set; }
    [Obsolete] public BorderDesign Border { get; set; }
}
```

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Name` | string | `""` | レイアウト名（スクリプトから参照用） |
| `Padding` | ThicknessDesign | `{}` | 内側余白 `{ Left, Top, Right, Bottom }` |
| `IsBordered` | bool | `false` | 枠線を表示するか |
| `UseBorderedShrinkWrap` | bool | `false` | 枠線時にコンテンツに合わせて縮小するか |
| `IsExpandable` | bool | `false` | 折りたたみ可能にするか |
| `ExpanderLabel` | string | `""` | 折りたたみ時のラベル |
| `IsExpanderDefaultOpened` | bool | `false` | デフォルトで展開するか |
| `IsFlowLayout` | bool | `false` | フローレイアウト（折り返し配置）にするか |
| `IsAutoFillWrap` | bool | `false` | 均等折り返し。MinWidth必須。MaxWidthは無効 |
| `IsFillAvailable` | bool | `false` | 利用可能な領域を埋めるか |
| `ScrollDirection` | ScrollDirection | `"Unset"` | スクロール方向: `Unset` / `Vertical` / `Horizontal` |
| `BackgroundColor` | string | `""` | 背景色 |
| `Rows` | List\<GridRow\> | `[]` | 行定義 |

### GridRow プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `IsWrap` | bool | `false` | 列が溢れた場合に折り返すか |
| `IsAutoFillWrap` | bool | `false` | 均等折り返し。MinWidth必須。MaxWidthは無効 |
| `Height` | double? | null | 行の高さ（px）。null で自動。 |
| `Margin` | ThicknessDesign | `{}` | 行の外側余白 |
| `GridRowType` | GridRowType | `"Normal"` | 行種別: `Normal` / `Header` / `Footer` |
| `CanResize` | bool | `false` | ユーザーによる行高さ変更を許可 |
| `BackgroundColor` | string | `""` | 行の背景色 |
| `IsRowMarginRemoved` | bool | `false` | 行間マージンを除去するか |
| `IsProportionalScale` | bool | `false` | 列幅を比率で拡大縮小。`true` でこの行の各列の `Width` を固定 px ではなく**比率**として扱い、行幅に合わせて比率を保ったままスケールする (例: `100/200/100` → 常に 25%/50%/25%)。**全列に `Width` 必須**、`MinWidth`/`MaxWidth`・`CanResize`・`IsWrap`/`IsAutoFillWrap`・`IsFlowLayout` とは併用不可 (デザインチェックで検証)。サンプル: `Samples/PatternShowcase/App/Modules/ProportionalScaleSample.mod.json` |
| `KeepInFillAvailableGrid` | bool | `false` | **基本 `false` のまま。`true` 化は超絶レア** (詳細は [CommonMistakes.md](CommonMistakes.md) #49 を必読)。`IsFillAvailable=true` の Grid の最終行 (FillAvailable target) が「ListField の内部スクロールでも `ProCode` の自前スクロールでもなく、`Button` や `Label` のような中身が固定高さの要素」のときに限り `true` にする。`ListField` や `ProCodeField` の最終行に `true` を立てるとモードが切り替わって**画面下端まで広がらなくなる**ので絶対やってはいけない |
| `Columns` | List\<GridColumn\> | `[]` | 列定義 |

### GridColumn プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Layout` | LayoutDesignBase | null | 配置するレイアウト（FieldLayoutDesign またはネストされたレイアウト）。null の場合は空セル。 |
| `Width` | double? | null | 列幅（px）。null で自動（残り領域を均等分割）。行が `IsProportionalScale: true` のときは比率として扱われる。 |
| `MinWidth` | double? | null | 最小幅（px）。flex:1と組み合わせて均等配置。IsAutoFillWrapで使用 |
| `MaxWidth` | double? | null | 最大幅（px）。MinWidthと組み合わせて伸びすぎを防止。※IsAutoFillWrap時は無効 |
| `Padding` | ThicknessDesign | `{}` | セルの内側余白 |
| `BackgroundColor` | string | `""` | セルの背景色 |
| `BorderStyle` | BorderStyleDesign | | セルの枠線スタイル（下記詳細参照） |
| `HorizontalAlignment` | HorizontalAlignment | | 水平配置: `Start` / `Center` / `End` / `Stretch` |
| `VerticalAlignment` | VerticalAlignment | | 垂直配置: `Top` / `Middle` / `Bottom` / `Stretch` |
| `CanResize` | bool | `false` | ユーザーによる列幅変更を許可 |

> レイアウト作成時の推奨ルールは [LayoutGuidelines.md](LayoutGuidelines.md) を参照。

### JSON例

```json
{
  "Name": "",
  "Padding": { "Left": 10, "Top": 10, "Right": 10, "Bottom": 10 },
  "IsBordered": true,
  "IsExpandable": false,
  "IsFlowLayout": false,
  "ScrollDirection": "Unset",
  "BackgroundColor": "#f5f5f5",
  "Rows": [
    {
      "IsWrap": false,
      "Height": null,
      "GridRowType": "Normal",
      "CanResize": false,
      "BackgroundColor": "",
      "Columns": [
        {
          "Layout": {
            "FieldName": "NameLabel",
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
          },
          "Width": 150,
          "Padding": {},
          "BackgroundColor": "",
          "VerticalAlignment": "Middle",
          "HorizontalAlignment": "Start",
          "CanResize": false
        },
        {
          "Layout": {
            "FieldName": "Name",
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
          },
          "Width": null,
          "CanResize": false
        }
      ]
    },
    {
      "IsWrap": false,
      "Height": null,
      "GridRowType": "Normal",
      "Columns": [
        {
          "Layout": {
            "FieldName": "PriceLabel",
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
          },
          "Width": 150,
          "VerticalAlignment": "Middle"
        },
        {
          "Layout": {
            "FieldName": "Price",
            "Name": "",
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
          },
          "Width": null
        }
      ]
    }
  ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
}
```

---

## 5. FieldLayoutDesign - フィールド配置

GridLayoutDesign の Column.Layout に指定して、フィールドを配置する。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign`

### C# クラス定義 (真実の源)

```csharp
public class FieldLayoutDesign : LayoutDesignBase, IFontAppearance
{
    public string FieldName { get; set; } = string.Empty;
    public string ContextMenu { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public int? FontSize { get; set; }
    public CssFontWeight? FontWeight { get; set; }
    public CssFontStyle? FontStyle { get; set; }
    public string Color { get; set; } = string.Empty;
    // 親 LayoutDesignBase から継承: Name, IsViewOnly, BackgroundColor
}
```

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `FieldName` | string | `""` | 配置するフィールドの Name を指定 |
| `IsViewOnly` | bool? | null | 読み取り専用で表示。`null` の場合は親レイアウトの設定を継承。 |
| `ContextMenu` | string | `""` | 関連する ContextMenuField の名前 |
| `ClassName` | string | `""` | CSSクラス名 |
| `FontFamily` | string | `""` | フォントファミリー（指定なしは親からカスケード） |
| `FontSize` | int? | null | フォントサイズ（指定なしは親からカスケード）。**整数のみ（`14.0` は不可、`14` と書くこと）** |
| `FontWeight` | CssFontWeight? | null | フォント太さ。**カスケード対象外** — 明示指定したフィールドにのみ適用される |
| `FontStyle` | CssFontStyle? | null | フォントスタイル。**カスケード対象外** — 明示指定したフィールドにのみ適用される |
| `Color` | string | `""` | 文字色（指定なしは親からカスケード） |
| `BackgroundColor` | string | `""` | 背景色（指定なしは親からカスケード） |

> **カスケード対象**は `Color` / `BackgroundColor` / `FontFamily` / `FontSize` の **4 つだけ**。`FontWeight` / `FontStyle` は親レイアウトに値があっても継承しない。太字や斜体を全体に効かせたい場合は対象フィールドを 1 つずつ指定するか、`app.css` で CSS から指定する。

### JSON例

```json
{
  "FieldName": "ProductName",
  "ContextMenu": "",
  "ClassName": "highlight-field",
  "FontFamily": "",
  "FontSize": 14,
  "FontWeight": "Bold",
  "Color": "#333333",
  "BackgroundColor": "",
  "Name": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
}
```

---

## 6. TabLayoutDesign - タブレイアウト

タブ切り替え式のレイアウト。各タブに別々のレイアウトを配置できる。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.TabLayoutDesign`

### C# クラス定義 (真実の源)

```csharp
public class TabLayoutDesign : LayoutDesignBase
{
    public override string Name { get; set; } = string.Empty;
    public List<string> Tabs { get; set; } = ["Tab 1"];           // 配列 (object NG)
    public ThicknessDesign Padding { get; set; } = new();
    public bool IsBordered { get; set; }
    public string Color { get; set; } = string.Empty;
    public string SelectedColor { get; set; } = string.Empty;
    public virtual List<LayoutDesignBase> Layouts { get; set; }    // 配列 (object NG)
        = [new GridLayoutDesign { Rows = [GridRow.CreateEmptyRow()] }];
    public string OnSelectedIndexChanged { get; set; } = string.Empty;
    public string OnSelectedIndexChanging { get; set; } = string.Empty;
    // 親 LayoutDesignBase から継承: IsViewOnly, BackgroundColor
}
```

> **注意 (Claude 向け)**: `Tabs` も `Layouts` も `List<T>` = JSON では **配列** (`[...]`)。`Layouts` を `{"0": ..., "1": ...}` のオブジェクトで書くとデシリアライズに失敗してモジュールがデザイナで認識されなくなる。`Tabs` と `Layouts` は同じ要素数で、`Tabs[i]` のラベルに対応するレイアウトが `Layouts[i]`。

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Tabs` | List\<string\> | `[]` | タブのラベル名リスト |
| `Layouts` | List\<LayoutDesignBase\> | `[]` | 各タブに対応するレイアウト。Tabs と同じ数だけ必要。 |
| `Padding` | ThicknessDesign | `{}` | 内側余白 |
| `IsBordered` | bool | `false` | 枠線を表示するか |
| `Color` | string | `""` | 通常タブの文字色 |
| `SelectedColor` | string | `""` | 選択中タブの文字色 |
| `OnSelectedIndexChanged` | string | `""` | タブ切り替え後のスクリプトイベント名 |
| `OnSelectedIndexChanging` | string | `""` | タブ切り替え前のスクリプトイベント名。`false` 返却でキャンセル可能。 |

### スクリプトAPI

| プロパティ | 型 | 説明 |
|---|---|---|
| `SelectedIndex` | int | 選択中のタブインデックス |

### スクリプトイベントハンドラ

#### OnSelectedIndexChanging

タブ切り替え前に呼ばれる。`false` を返すとタブ切り替えをキャンセル。

```csharp
bool Tab_OnChanging()
{
    // バリデーション等を行い、切り替え可否を判定
    if (!this.ValidateInput())
    {
        MessageBox.Show("入力エラーがあります");
        return false;
    }
    return true;
}
```

#### OnSelectedIndexChanged

タブ切り替え後に呼ばれる。

```csharp
void Tab_OnChanged()
{
    // 切り替え後のタブに応じた処理
    Logger.Log("タブが切り替わりました");
}
```

### JSON例

```json
{
  "Tabs": ["基本情報", "詳細情報", "履歴"],
  "Layouts": [
    {
      "Rows": [
        {
          "Columns": [
            {
              "Layout": {
                "FieldName": "Name",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              }
            }
          ]
        }
      ],
      "IsBordered": false,
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
    },
    {
      "Rows": [
        {
          "Columns": [
            {
              "Layout": {
                "FieldName": "Description",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              }
            }
          ]
        }
      ],
      "IsBordered": false,
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
    },
    {
      "Rows": [
        {
          "Columns": [
            {
              "Layout": {
                "FieldName": "HistoryList",
                "Name": "",
                "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
              }
            }
          ]
        }
      ],
      "IsBordered": false,
      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
    }
  ],
  "Padding": {},
  "IsBordered": true,
  "Color": "#666666",
  "SelectedColor": "#0066cc",
  "OnSelectedIndexChanged": "",
  "OnSelectedIndexChanging": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.TabLayoutDesign"
}
```

---

## 7. CanvasLayoutDesign - キャンバスレイアウト

絶対位置指定でフィールドを自由に配置するレイアウト。

`TypeFullName`: `Codeer.LowCode.Blazor.Repository.Design.CanvasLayoutDesign`

### C# クラス定義 (真実の源)

```csharp
public class CanvasLayoutDesign : LayoutDesignBase
{
    public List<CanvasElement> Elements { get; set; } = new();
    public bool IsBordered { get; set; }
    public ScrollDirection ScrollDirection { get; set; }  // enum: Unset / Vertical / Horizontal
    // 親 LayoutDesignBase から継承: Name, IsViewOnly, BackgroundColor
}

public class CanvasElement
{
    public LayoutDesignBase? Layout { get; set; }  // FieldLayoutDesign / Grid / Tab / Canvas
    public double Left { get; set; }
    public double Top { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public int? ZIndex { get; set; }
    // CanvasElement 自体には Name プロパティは存在しない
}
```

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Elements` | List\<CanvasElement\> | `[]` | 配置する要素のリスト。モジュールの Fields 配列と同じ順番で対応する。 |
| `IsBordered` | bool | `false` | 枠線を表示するか |
| `ScrollDirection` | ScrollDirection | `"Unset"` | スクロール方向 |
| `BackgroundColor` | string | `""` | 背景色 |

### CanvasElement プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Layout` | FieldLayoutDesign | | フィールドレイアウト（省略時は Fields の順序で自動対応） |
| `Left` | double | 0 | 左端からの距離（px） |
| `Top` | double | 0 | 上端からの距離（px） |
| `Width` | double? | null | 要素の幅（px）。null で自動。 |
| `Height` | double? | null | 要素の高さ（px）。null で自動。 |
| `ZIndex` | int? | null | 重なり順序。**整数のみ（`10.0` は不可、`10` と書くこと）** |

> **CanvasElement 自体には `Name` プロパティが無い**ため、スクリプトから個別の Element を直接操作することはできない。Element 内に Layout（GridLayoutDesign / TabLayoutDesign / CanvasLayoutDesign）をネストし、その Layout の `Name` 経由でアクセスすること。

### JSON例

```json
{
  "Elements": [
    {
      "Layout": {
        "FieldName": "TitleLabel",
        "Name": "",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
      },
      "Left": 20,
      "Top": 10,
      "Width": 300,
      "Height": null
    },
    {
      "Layout": {
        "FieldName": "Logo",
        "Name": "",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
      },
      "Left": 500,
      "Top": 10,
      "Width": 200,
      "Height": 100,
      "ZIndex": 1
    },
    {
      "Layout": {
        "FieldName": "ActionButton",
        "Name": "",
        "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
      },
      "Left": 20,
      "Top": 150,
      "Width": 150
    }
  ],
  "IsBordered": true,
  "ScrollDirection": "Unset",
  "BackgroundColor": "#fafafa",
  "Name": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.CanvasLayoutDesign"
}
```

---

## レイアウトのネスト

GridLayoutDesign の Column.Layout には FieldLayoutDesign だけでなく、GridLayoutDesign、TabLayoutDesign、CanvasLayoutDesign もネストして配置できる。これにより複雑なレイアウトを実現できる。

```json
{
  "Rows": [
    {
      "Columns": [
        {
          "Layout": {
            "Rows": [
              {
                "Columns": [
                  {
                    "Layout": {
                      "FieldName": "InnerField1",
                      "Name": "",
                      "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
                    }
                  }
                ]
              }
            ],
            "IsBordered": true,
            "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
          },
          "Width": null
        }
      ]
    }
  ],
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
}
```

## ThicknessDesign 構造

Padding や Margin で使用される。

### C# クラス定義 (真実の源)

```csharp
public class ThicknessDesign
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Right { get; set; }
    public double? Bottom { get; set; }
}
```

```json
{
  "Left": 10,
  "Top": 5,
  "Right": 10,
  "Bottom": 5
}
```

省略されたプロパティは 0 として扱われる。空のオブジェクト `{}` は余白なしを意味する。

## BorderStyleDesign 構造

GridColumn の `BorderStyle` で使用される。枠線の幅と色を各辺ごとに設定できる。

### C# クラス定義 (真実の源)

```csharp
public class BorderStyleDesign
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Right { get; set; }
    public double? Bottom { get; set; }
    public string LeftColor { get; set; } = string.Empty;
    public string TopColor { get; set; } = string.Empty;
    public string RightColor { get; set; } = string.Empty;
    public string BottomColor { get; set; } = string.Empty;
}
```

### プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Left` | double? | null | 左枠線の幅（px） |
| `Top` | double? | null | 上枠線の幅（px） |
| `Right` | double? | null | 右枠線の幅（px） |
| `Bottom` | double? | null | 下枠線の幅（px） |
| `LeftColor` | string | `""` | 左枠線の色（CSS色値） |
| `TopColor` | string | `""` | 上枠線の色（CSS色値） |
| `RightColor` | string | `""` | 右枠線の色（CSS色値） |
| `BottomColor` | string | `""` | 下枠線の色（CSS色値） |

### JSON例

```json
{
  "BorderStyle": {
    "Left": 1,
    "Top": 1,
    "Right": 1,
    "Bottom": 2,
    "LeftColor": "#cccccc",
    "TopColor": "#cccccc",
    "RightColor": "#cccccc",
    "BottomColor": "#333333"
  }
}
```

省略されたプロパティは枠線なしを意味する。

## IsViewOnly（レイアウト共通）

全てのレイアウト型（GridLayoutDesign, TabLayoutDesign, CanvasLayoutDesign, FieldLayoutDesign）と ListElement は `IsViewOnly` プロパティ（`bool?`）を持つ。`true` に設定すると、そのレイアウト内の全フィールドが読み取り専用になる。`null` の場合は親の設定を継承する。

### 設定場所の階層と優先順位

IsViewOnly は以下の階層で設定可能。**子要素の設定が親要素の設定を上書きする。**

```
DetailLayoutDesign
  └── GridLayoutDesign (IsViewOnly)       ← グリッド全体を閲覧専用にする
       └── FieldLayoutDesign (IsViewOnly) ← 個別フィールドの閲覧専用を制御

ListLayoutDesign
  └── ListElement (IsViewOnly)            ← 個別列の閲覧専用を制御

TabLayoutDesign (IsViewOnly)              ← タブ全体を閲覧専用にする
  └── 各タブの Layout (IsViewOnly)        ← 個別タブの閲覧専用を制御
```

### 重要: IsViewOnly はフィールド定義のプロパティではない

`IsViewOnly` はレイアウト要素（FieldLayoutDesign, GridLayoutDesign, ListElement 等）のプロパティであり、**フィールド定義（Fields配列内）のプロパティではない**。フィールド定義に書いてもデシリアライズ時に無視される。

```json
// ❌ 誤り: フィールド定義に設定しても効かない
"Fields": [
  {
    "DbColumn": "total",
    "IsViewOnly": true,
    "Name": "Total",
    "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign"
  }
]

// ✅ 正しい: FieldLayoutDesign に設定
{
  "FieldName": "Total",
  "IsViewOnly": true,
  "Name": "",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.FieldLayoutDesign"
}

// ✅ 正しい: GridLayoutDesign に設定（配下全フィールドが閲覧専用）
{
  "Rows": [...],
  "IsViewOnly": true,
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.GridLayoutDesign"
}

// ✅ 正しい: ListElement に設定
{
  "FieldName": "Total",
  "Label": "合計",
  "IsViewOnly": true
}
```

