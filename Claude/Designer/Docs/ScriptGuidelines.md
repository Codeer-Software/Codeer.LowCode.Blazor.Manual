# スクリプト作成ガイドライン

Claude Code でスクリプト (`.mod.cs`) を作成する際の規約・注意事項。

---

## 明細の集計はサーバー問い合わせではなく画面上のデータを使う

DetailListField / ListField の行データを集計する場合、`ModuleSearcher` でDBに問い合わせるのではなく、フィールドの `Rows` プロパティから現在の画面上のデータを使って計算する。

### 理由

- `ModuleSearcher` はDBに保存済みのデータしか返さない
- ユーザーが編集中（未保存）の明細行が反映されない
- 不要なサーバーアクセスが発生する

### 正しい例

```csharp
void Items_OnDataChanged()
{
    decimal total = 0;
    foreach (var row in Items.Rows)
    {
        var item = (ExpenseItem)row;
        if (item.Amount.Value != null)
        {
            total += item.Amount.Value;
        }
    }
    TotalAmount.Value = total;
}
```

### 誤った例

```csharp
// ❌ DBから取得し直すのは不適切
void Items_OnDataChanged()
{
    var search = new ModuleSearcher<ExpenseItem>();
    search.AddEquals(e => e.ExpenseReportId.Value, Id.Value);
    var items = search.Execute();

    decimal total = 0;
    foreach (var item in items)
    {
        if (item.Amount.Value != null)
        {
            total += item.Amount.Value;
        }
    }
    TotalAmount.Value = total;
}
```

### 補足

- `Items.Rows` は `List<Module>` 型。具象型にキャストして使う（例: `(ExpenseItem)row`）
- `ModuleSearcher` は、画面上に無いデータを検索する場合（マスタ参照、別モジュールの検索等）に使う

---

## スクリプトから使えるフィールドプロパティ

スクリプト (`.mod.cs`) から設定可能なフィールドプロパティは以下のみ:

- `Color`, `BackgroundColor` - 表示色
- `IsEnabled` - 有効/無効
- `IsVisible` - 表示/非表示
- `IsViewOnly` - 閲覧専用
- `IsValid`, `ErrorText` - バリデーション状態
- `ClassName` - CSSクラス
- `IgnoreModification` - 変更検知除外

### ⚠️ `IsRequired` はスクリプトから設定できない

`IsRequired` はデザイン時プロパティ（JSON で定義）であり、ランタイムスクリプトからはアクセスできない。

条件付きで必須にしたい場合は、`IsVisible` で表示/非表示を切り替える方法で対応する。

```csharp
// ❌ エラー: IsRequired はスクリプトから使えない
CustomerId.IsRequired = true;

// ✅ 正しい: IsVisible で表示切り替え + 値クリア
CustomerId.IsVisible = isEntertainment;
if (!isEntertainment)
{
    CustomerId.Value = null;
}
```

---

## this でモジュールメソッドを呼ぶ

モジュール自体のメソッド（Submit, Delete, Reload, ValidateInput 等）は `this.` を付けて呼ぶ。

```csharp
// ❌ 誤り: this がないと見つからない
Submit();
ValidateInput();

// ✅ 正しい
this.Submit();
this.ValidateInput();
this.Reload();
this.Delete();
this.NewModule();
this.CopyModule();
```

フィールドへのアクセスは `this.` 不要（暗黙的にモジュールスコープ）。

```csharp
Name.Value = "テスト";    // OK
this.Name.Value = "テスト"; // これもOK（明示的）
```

---

## IsNewData で新規/既存を判定する

新規レコード（まだDBに保存されていない）かどうかは `this.IsNewData` で判定する。

```csharp
void Detail_OnAfterInit()
{
    if (this.IsNewData)
    {
        // 新規データの場合のデフォルト値
        Status.Value = "Draft";
        CreatedDate.Value = DateOnly.FromDateTime(DateTime.Today);
    }
    else
    {
        // 既存データの場合
        EditPanel.IsViewOnly = true;
    }
}
```

---

## 保存前にバリデーションを行う

カスタム保存処理を書く場合、`this.ValidateInput()` で全フィールドを検証してから `this.Submit()` する。

```csharp
void SaveButton_OnClick()
{
    // 全フィールドのバリデーション
    if (!this.ValidateInput())
    {
        MessageBox.Show("入力エラーがあります。修正してください。");
        return;
    }

    // カスタムバリデーション
    if (StartDate.Value != null && EndDate.Value != null && StartDate.Value > EndDate.Value)
    {
        EndDate.SetError("終了日は開始日以降にしてください");
        return;
    }

    this.Submit();
    MessageBox.Show("保存しました");
}
```

---

## ModuleSearcher の正しい使い方

`ModuleSearcher` は別モジュールのDBデータを検索するためのもの。画面上のデータではなくDB保存済みデータを返す。

### 正しい用途

- マスタ参照（別モジュールのデータ取得）
- SelectField/LinkField の候補フィルタリング
- 他モジュールのデータ検索

```csharp
// マスタ参照: 商品の単価を取得
var searcher = new ModuleSearcher<Product>();
searcher.AddEquals(e => e.Code.Value, ProductCode.Value);
var products = searcher.Execute();
if (products.Count > 0)
{
    UnitPrice.Value = products[0].Price.Value;
}

// SelectFieldの候補フィルタリング
var catSearcher = new ModuleSearcher<SubCategory>();
catSearcher.AddEquals(e => e.CategoryId.Value, Category.Value);
SubCategory.SetAdditionalCondition(catSearcher);
SubCategory.ReloadCandidates();
```

### 誤った用途

- 画面上の明細行の集計 → `Rows` プロパティを使う（[CommonMistakes.md](CommonMistakes.md) の #4 参照）

---

## 帳票出力パターン

Excelテンプレートを使った帳票出力は2パターンある。`using (var memory = ...)` ブロック形式を使い、Excelコンストラクタの第2引数はファイル名（拡張子なし）。

### パターン1: Resources のテンプレートを使用

```csharp
void PdfButton_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Invoice.xlsx"))
    {
        var excel = new Excel(memory, "Invoice");
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}
```

### パターン2: FileField に登録されたファイルを使用

```csharp
void ExcelButton_OnClick()
{
    using (var memory = Template.GetMemoryStream())
    {
        var excel = new Excel(memory, "Report");
        excel.OverWrite(this);
        excel.Download();
    }
}
```

### 出力メソッド

| メソッド | 出力形式 |
|---|---|
| `excel.Download()` | Excel (.xlsx) |
| `excel.DownloadPdf()` | PDF |

### OverWrite のプレースホルダー書式

`excel.OverWrite(this)` は、Excelテンプレート内の `{{FieldName.Property}}` 形式のプレースホルダーをモジュールのフィールド値で置換する。

```
{{Name.Value}}           → TextField の値
{{Price.Value}}          → NumberField の値
{{CreatedDate.Value}}    → DateField の値
{{Category.DisplayText}} → LinkField の表示テキスト
```

### セルの個別操作

`OverWrite` ではカバーできない場合、セルを個別に操作できる。

```csharp
void CustomExport_OnClick()
{
    using (var memory = Resources.GetMemoryStream("Template.xlsx"))
    {
        var excel = new Excel(memory, "CustomReport");

        // セルをテキストで検索して値を設定
        var cell = excel.FindCellByText("{{Title}}");
        if (cell != null)
        {
            excel.SetCellValue(cell, Title.Value);
        }

        // 行のコピー（テンプレート行を複製して明細を展開）
        var startCell = new ExcelCellIndex();
        startCell.RowIndex = 5;
        startCell.ColumnIndex = 0;
        var destCell = startCell.GetNext(1, 0);
        excel.CopyCells(startCell, destCell, 1, 5);

        excel.DownloadPdf();
    }
}
```

### 注意事項

- `Excel` は `IDisposable` のため、**必ず `using` ブロック内で使う**こと
- コンストラクタの第2引数（ファイル名）に**拡張子は不要**（`.xlsx` / `.pdf` は自動付与）
- `Download()` と `DownloadPdf()` は非同期メソッドだが、スクリプトエンジンが自動で await するため明示的な `await` は不要

### Excel メソッド一覧

全メソッドの詳細は [ScriptExtensions.md](ScriptExtensions.md) の Excel セクションを参照。

---

## Null条件演算子（?.）は使えない

スクリプトエンジンはNull条件演算子（`?.`、`?[]`）をサポートしていない。代わりに明示的なnullチェックを書く。

```csharp
// ❌ エラー: ?. はサポートされない
var name = product?.Name.Value;

// ✅ 正しい: 明示的にnullチェック
string name = null;
if (product != null)
{
    name = product.Name.Value;
}

// ✅ Null合体演算子（??）は使える
var value = Price.Value ?? 0;
```

---

## Rows の行を具象型にキャストする

ListField / DetailListField の `Rows` は `List<Module>` 型。各行を具象モジュール型にキャストしてフィールドにアクセスする。

```csharp
void Items_OnDataChanged()
{
    decimal total = 0;
    foreach (var row in Items.Rows)
    {
        var item = (OrderItem)row;  // 具象型にキャスト
        if (item.Amount.Value != null)
        {
            total += item.Amount.Value;
        }
    }
    TotalAmount.Value = total;
}
```
