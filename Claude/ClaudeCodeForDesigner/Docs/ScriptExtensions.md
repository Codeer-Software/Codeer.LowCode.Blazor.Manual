# スクリプト拡張サービス

スクリプトエンジンはアプリケーション側で拡張可能。`ScriptRuntimeTypeManager` に型やサービスを登録することで、
スクリプトから新しいクラスやサービスにアクセスできるようになる。

このドキュメントでは、標準アプリケーションテンプレート（`WebApp.Client.Shared`）で登録される拡張サービスを説明する。

---

## 拡張の仕組み

`Source/App/WebApp.Client.Shared/Services/AppInfoService.cs` で以下のように登録される。

```csharp
// 型の登録（new で生成可能になる）
_scriptRuntimeTypeManager.AddType(typeof(Excel));
_scriptRuntimeTypeManager.AddType(typeof(ExcelCellIndex));
_scriptRuntimeTypeManager.AddType<WebApiResult>();
_scriptRuntimeTypeManager.AddType<LoadingService.LoadingScope>();

// サービスの登録（スクリプト内でサービス名で直接アクセス可能）
_scriptRuntimeTypeManager.AddService(new WebApiService(http, logger));
_scriptRuntimeTypeManager.AddService(new Toaster(toaster));
_scriptRuntimeTypeManager.AddService(new MailService());
_scriptRuntimeTypeManager.AddService(loadingService);

// カスタムインジェクター（[ScriptInject]プロパティへの注入）
_scriptRuntimeTypeManager.AddCustomInjector(() => http);
```

---

## 標準拡張サービス一覧

| サービス / 型 | アクセス方法 | 説明 |
|---|---|---|
| `WebApiService` | サービス名で直接 | HTTP API呼び出し |
| `Toaster` | サービス名で直接 | トースト通知表示 |
| `MailService` | サービス名で直接 | メール送信 |
| `LoadingService` | サービス名で直接 | ローディング表示 |
| `Excel` | `new Excel(...)` | Excel操作 |
| `ExcelCellIndex` | `new ExcelCellIndex()` | Excelセル位置 |
| `WebApiResult` | 戻り値型 | WebAPI応答 |

---

## WebApiService

外部APIにHTTPリクエストを送信する。

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Get(string url)` | `WebApiResult` | GETリクエスト |
| `Post(string url, JsonObject data)` | `WebApiResult` | POSTリクエスト |
| `Put(string url, JsonObject data)` | `WebApiResult` | PUTリクエスト |
| `Delete(string url)` | `WebApiResult` | DELETEリクエスト |

### WebApiResult

| プロパティ | 型 | 説明 |
|---|---|---|
| `JsonObject` | JsonObject | レスポンスボディ（JSON） |
| `StatusCode` | int | HTTPステータスコード |

### 使用例

```csharp
// GETリクエスト
var result = WebApiService.Get("/api/products");
var data = result.JsonObject;

// POSTリクエスト
var body = new JsonObject();
body.Name = "新商品";
body.Price = 1000;
var result = WebApiService.Post("/api/products", body);
if (result.StatusCode == 200)
{
    Toaster.Success("登録しました");
}

// データの一括取得と表示
void FetchData_OnClick()
{
    var data = WebApiService.Get("/api/data").JsonObject;
    foreach (var e in data)
    {
        var row = new DataItem();
        row.Name.Value = e.Name;
        row.Value.Value = e.Value;
        DataList.AddRow(row);
    }
}
```

---

## Toaster

画面にトースト（一時通知）を表示する。

### メソッド

| メソッド | 説明 |
|---|---|
| `Success(string message)` | 成功トースト（緑） |
| `Warn(string message)` | 警告トースト（黄） |
| `Error(string message)` | エラートースト（赤） |

### 使用例

```csharp
void SaveButton_OnClick()
{
    this.Submit();
    Toaster.Success("保存しました");
}

void Delete_OnClick()
{
    var result = MessageBox.Show("削除しますか？", "はい", "いいえ");
    if (result == "はい")
    {
        this.Delete();
        Toaster.Warn("削除しました");
    }
}
```

---

## MailService

メールを送信する。サーバー側で `SendEmailAsyncCore` の設定が必要。

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `SendEmail(string address, string subject, string message)` | `bool` | メール送信。成功時 `true` |

### 使用例

```csharp
void SendNotification_OnClick()
{
    var success = MailService.SendEmail(
        Email.Value,
        "注文確認",
        "ご注文ありがとうございます。注文番号: " + OrderId.Value
    );
    if (success)
    {
        Toaster.Success("メールを送信しました");
    }
    else
    {
        Toaster.Error("メール送信に失敗しました");
    }
}
```

---

## LoadingService

ローディングスピナーを表示する。

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `StartLoading()` | `LoadingScope` | ローディング開始。`Dispose()` で終了 |
| `StartLoading(int? delayTime)` | `LoadingScope` | 遅延付きローディング開始 |

### 使用例

```csharp
void HeavyProcess_OnClick()
{
    using (var loading = LoadingService.StartLoading())
    {
        // 重い処理
        var search = new ModuleSearcher<Product>();
        var list = search.Execute();
        foreach (var item in list)
        {
            item.Status.Value = "Processed";
            item.Submit();
        }
    }
    // using を抜けるとローディング自動終了
    Toaster.Success("処理完了");
}
```

---

## Excel

Excelファイルの読み書きとダウンロード。`IDisposable` のため `using` で使用すること。

### コンストラクタ

```csharp
new Excel(MemoryStream? stream, string fileName)
```

### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `OverWrite(Module data)` | void | モジュールデータでExcelを上書き |
| `FindCellByText(string text)` | `ExcelCellIndex?` | テキストでセルを検索 |
| `SetCellValue(ExcelCellIndex cell, object value)` | void | セルに値を設定 |
| `CopyCells(ExcelCellIndex source, ExcelCellIndex dest, int rows, int cols)` | void | セル範囲コピー |
| `AddImage(ExcelCellIndex cell, Stream stream)` | void | 画像を挿入 |
| `Download()` | bool | xlsxとしてダウンロード |
| `DownloadPdf()` | bool | PDFに変換してダウンロード |

### ExcelCellIndex

| プロパティ | 型 | 説明 |
|---|---|---|
| `RowIndex` | int | 行インデックス |
| `ColumnIndex` | int | 列インデックス |
| `GetNext(int rowOffset, int colOffset)` | ExcelCellIndex | オフセットした新しい位置 |

### 使用例

```csharp
// テンプレートExcelに書き出してダウンロード
void ExportExcel_OnClick()
{
    var searchFile = new ModuleSearcher<TestFiles>();
    searchFile.AddEquals(e => e.Name.Value, "Template");
    var file = searchFile.Execute()[0];

    using (var memory = file.File.GetMemoryStream())
    using (var excel = new Excel(memory, file.File.FileName))
    {
        excel.OverWrite(this);
        excel.Download();
    }
}

// PDFとしてダウンロード
void ExportPdf_OnClick()
{
    var searchFile = new ModuleSearcher<TestFiles>();
    searchFile.AddEquals(e => e.Name.Value, "Template");
    var file = searchFile.Execute()[0];

    using (var memory = file.File.GetMemoryStream())
    using (var excel = new Excel(memory, file.File.FileName))
    {
        excel.OverWrite(this);
        excel.DownloadPdf();
    }
}

// セルを個別に操作
void CustomExport_OnClick()
{
    var searchFile = new ModuleSearcher<TestFiles>();
    searchFile.AddEquals(e => e.Name.Value, "Template");
    var file = searchFile.Execute()[0];

    using (var memory = file.File.GetMemoryStream())
    using (var excel = new Excel(memory, file.File.FileName))
    {
        var cell = excel.FindCellByText("{{Name}}");
        if (cell != null)
        {
            excel.SetCellValue(cell, Name.Value);
        }
        excel.Download();
    }
}
```

---

## 拡張の追加方法

プロジェクト固有のサービスやクラスを追加するには、`AppInfoService.cs` で `ScriptRuntimeTypeManager` に登録する。

### サービスの追加（スクリプト内でサービス名で直接アクセス）

```csharp
// 1. サービスクラスを作成
public class MyService
{
    [ScriptInject]
    public Services? Services { get; set; }

    [ScriptInject]
    public HttpService? Http { get; set; }

    [ScriptName("DoSomething")]
    public async Task<string> DoSomethingAsync(string input)
    {
        // 実装
        return $"Processed: {input}";
    }

    [ScriptHide]
    public void InternalMethod() { }  // スクリプトから非表示
}

// 2. AppInfoService で登録
_scriptRuntimeTypeManager.AddService(new MyService());
```

スクリプト側:
```csharp
var result = MyService.DoSomething("test");
```

### 型の追加（スクリプト内で `new` で生成可能）

```csharp
// 1. クラスを作成
public class MyData
{
    public string Name { get; set; } = "";
    public int Value { get; set; }

    public string Format()
    {
        return $"{Name}: {Value}";
    }
}

// 2. AppInfoService で登録
_scriptRuntimeTypeManager.AddType<MyData>();
```

スクリプト側:
```csharp
var data = new MyData();
data.Name = "test";
data.Value = 100;
Logger.Log(data.Format());
```

### カスタムインジェクター

`[ScriptInject]` 属性が付いたプロパティに自動注入される値を登録する。

```csharp
_scriptRuntimeTypeManager.AddCustomInjector(() => myDependency);
```

---

## スクリプト属性リファレンス

拡張クラスで使用可能な属性。

| 属性 | 適用先 | 説明 |
|---|---|---|
| `[ScriptName("Name")]` | メソッド, プロパティ | スクリプト内での名前を変更。例: `GetAsync` → `Get` |
| `[ScriptHide]` | メソッド, プロパティ | スクリプトから非表示にする |
| `[ScriptInject]` | プロパティ | フレームワークサービスを自動注入 |
| `[ScriptMethodToProperty("Name")]` | メソッド | 非同期メソッドをプロパティとして公開。例: `SetValueAsync` → `Value` のセッター |

---

## アプリバリアント別の対応状況

| バリアント | Excel | WebApi | Toaster | Mail | Loading |
|---|---|---|---|---|---|
| Blazor WASM (Cookie等) | ○ | ○ | ○ | ○ | ○ |
| Server-Side Blazor | ○ | ○ | ○ | - | - |
| WPF Desktop | ○ | ○ | ○ | - | - |
| WinForms Desktop | ○ | ○ | ○ | - | - |
