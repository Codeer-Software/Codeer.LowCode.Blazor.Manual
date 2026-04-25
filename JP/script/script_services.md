# 組み込みサービスとテンプレート由来サービス

スクリプトから呼び出せるサービスは、提供元によって 2 種類に分かれます。

| 区分 | 提供元 | カスタマイズ |
|---|---|---|
| **組み込みサービス** | `Codeer.LowCode.Blazor` 本体 | 不可（言語仕様の一部） |
| **テンプレート由来サービス** | VS テンプレートが出力する `WebApp.Client.Shared` | **書き換え・追加・削除すべて可能** |

テンプレート由来サービスは `AppInfoService` の中で `AddType` / `AddService` で登録されています。
独自に追加したい場合は [スクリプトの拡張](script_extend.md) を参照してください。

---

## 組み込みサービス

### Logger — デバッグログ

ブラウザの開発者ツールの Console に出力します（画面には出ません）。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Logger.Log(string)` | `Task` | info レベル |
| `Logger.Warn(string)` | `Task` | warning レベル |
| `Logger.Error(string)` | `Task` | error レベル |

```csharp
await Logger.Log("デバッグ情報: " + Name.Value);
```

### MessageBox — モーダルダイアログ

OK が押されるまで処理を待ちます。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `MessageBox.Show(string message, params string[] buttons)` | `Task<string>` | 押されたボタン名を返す |
| `MessageBox.ShowWithTitle(string title, string message, params string[] buttons)` | `Task<string>` | タイトル付き |
| `MessageBox.Show(string message, params DialogButton[] buttons)` | `Task<string>` | スタイル付きボタン |
| `MessageBox.ShowWithTitle(string title, string message, params DialogButton[] buttons)` | `Task<string>` | タイトル + スタイル付き |

```csharp
var result = await MessageBox.Show("削除しますか？", "はい", "いいえ");
if (result == "はい") { ... }

await MessageBox.Show("注意",
    new DangerButton { Text = "削除" },
    new SecondaryButton { Text = "キャンセル" });
```

ボタンの種類は `PrimaryButton` / `SecondaryButton` / `SuccessButton` / `DangerButton` / `WarningButton` / `InfoButton` / `LightButton` / `DarkButton` / `LinkButton`（および各 `*OutlineButton`）から選べます。

### NavigationService — 画面遷移と URL

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `NavigationService.NavigateTo(string url)` | `void` | 画面遷移 |
| `NavigationService.ReplaceTo(string url)` | `void` | 履歴を置き換えて遷移 |
| `NavigationService.GetModuleUrl(string moduleSegment)` | `string` | モジュールトップへの URL を組み立てる |
| `NavigationService.GetModuleUrl(string pageFrameSegment, string moduleSegment)` | `string` | PageFrame 指定版 |
| `NavigationService.GetModuleDataUrl(string moduleSegment, string idSegment)` | `string` | 個別データへの URL |
| `NavigationService.GetModuleDataUrl(string pageFrameSegment, string moduleSegment, string idSegment)` | `string` | PageFrame 指定版 |
| `NavigationService.GetQueryParameters()` | `Dictionary<string, List<string>>` | クエリパラメータ |
| `NavigationService.GetUniqueQueryParameters()` | `Dictionary<string, string>` | キーに対し最初の値だけ取る |
| `NavigationService.GetQueryString()` | `string` | クエリ文字列を組み立て直す |
| `NavigationService.Logout()` | `Task` | ログアウト |

| プロパティ | 型 | 説明 |
|---|---|---|
| `NavigationService.PageFrameUrlSegment` | `string` | 現在の PageFrame セグメント |
| `NavigationService.ModuleUrlSegment` | `string` | 現在のモジュールセグメント |

```csharp
NavigationService.NavigateTo(NavigationService.GetModuleUrl("Customer"));

var qs = NavigationService.GetUniqueQueryParameters();
if (qs.TryGetValue("id", out var id)) { ... }
```

### Resources — リソースアクセス

`Resources/` フォルダ配下のファイルを読みます。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Resources.GetMemoryStream(string path)` | `Task<MemoryStream?>` | バイナリで取得 |
| `Resources.GetText(string path)` | `Task<string>` | UTF-8 テキストで取得 |
| `Resources.Localize(string text)` | `string` | 多言語リソースの引き当て |

```csharp
using var stream = await Resources.GetMemoryStream("Templates/Invoice.xlsx");
var template = await Resources.GetText("Templates/MailBody.txt");
```

### BatchSearcher — 複数検索を 1 回のリクエストで実行

`ModuleSearcher` を複数まとめて投げて、サーバーラウンドトリップを節約します。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `BatchSearcher.Execute(params ModuleSearcher[])` | `Task<BatchModuleSearchResponse>` | 複数検索を一括実行 |

```csharp
var s1 = new ModuleSearcher<Customer>();
var s2 = new ModuleSearcher<Order>();
var response = await BatchSearcher.Execute(s1, s2);

var customers = response.GetAt(0);
var orders = response.GetAt(1);
```

詳細は [ModuleSearcher / BatchSearcher](script_module_searcher.md) を参照。

### PageFrameService — PageFrame の状態

カスタムサイドバー / ヘッダーを `Module` で実装したときの参照と、サイドバー幅の制御に使います。

| プロパティ | 型 | 説明 |
|---|---|---|
| `PageFrameService.LeftSideBarModule` | `Module?` | 左サイドバーとして埋め込まれている Module |
| `PageFrameService.RightSideBarModule` | `Module?` | 右サイドバーとして埋め込まれている Module |
| `PageFrameService.HeaderModule` | `Module?` | ヘッダーとして埋め込まれている Module |
| `PageFrameService.LeftSideBarState` | `SideBarState` | 左サイドバーの状態（`Width` 設定） |
| `PageFrameService.RightSideBarState` | `SideBarState` | 右サイドバーの状態 |

```csharp
PageFrameService.LeftSideBarState.SetWidth(280);
```

---

## テンプレート由来サービス

`Codeer.LowCode.Blazor.Templates` の VS テンプレートで作成したプロジェクトには、`WebApp.Client.Shared/ScriptObjects/` 配下に以下のサービスが含まれます。**ユーザコード側にあるので、必要に応じて拡張できます**。

### Toaster — トースト通知

画面右下に一時的なメッセージを出します（非ブロッキング）。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Toaster.Success(string)` | `void` | 成功通知 |
| `Toaster.Warn(string)` | `void` | 警告通知 |
| `Toaster.Error(string)` | `void` | エラー通知 |

```csharp
Toaster.Success("保存しました");
Toaster.Error("入力エラーがあります");
```

### WebApiService — HTTP 呼び出し

外部 API や同一プロジェクトの追加 Controller を呼び出します。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `WebApiService.Get(string url)` | `Task<WebApiResult>` | GET |
| `WebApiService.Post(string url, JsonObject data)` | `Task<WebApiResult>` | POST |
| `WebApiService.Put(string url, JsonObject data)` | `Task<WebApiResult>` | PUT |
| `WebApiService.Delete(string url)` | `Task<WebApiResult>` | DELETE |

`WebApiResult` のプロパティ:

| プロパティ | 型 | 説明 |
|---|---|---|
| `StatusCode` | `int` | HTTP ステータス |
| `JsonObject` | `JsonObject` | レスポンス本文を JSON として保持 |

```csharp
var result = await WebApiService.Get("/testapi/weather");
if (result.StatusCode == 200)
{
    foreach (var e in result.JsonObject)
    {
        var row = new WeatherForecast();
        row.Date.Value = e.Date;
        WeatherList.AddRow(row);
    }
}
```

### MailService — メール送信

サーバー側の `/api/mail` を呼び出してメール送信します。`appsettings.json` の `MailSettings` を設定する必要があります。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `MailService.SendEmail(string address, string subject, string message)` | `Task<bool>` | 成功/失敗 |

```csharp
var ok = await MailService.SendEmail("user@example.com", "件名", "本文");
if (!ok) Toaster.Error("送信に失敗しました");
```

### Excel — Excel テンプレートで帳票を作って Excel または PDF で配布

帳票テンプレート（`.xlsx`）を読み込み、Module の値で穴埋めしてダウンロードさせる仕組みです。

#### 生成

```csharp
using var stream = await Resources.GetMemoryStream("Templates/Invoice.xlsx");
using var excel = new Excel(stream, "請求書.xlsx");
```

#### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `OverWrite(Module data)` | `Task` | テンプレート内の `{Field名}` プレースホルダを Module の値で置換 |
| `FindCellByText(string text)` | `ExcelCellIndex?` | 指定テキストに一致するセルを検索 |
| `SetCellValue(ExcelCellIndex cell, object value)` | `void` | セルに値をセット |
| `CopyCells(ExcelCellIndex source, ExcelCellIndex destination, int rowCount, int colCount)` | `void` | セル範囲をコピー |
| `AddImage(ExcelCellIndex cellIndex, Stream stream)` | `void` | 画像を貼り付け |
| `Download()` | `Task<bool>` | Excel 形式でダウンロード（拡張子 `.xlsx`） |
| `DownloadPdf()` | `Task<bool>` | PDF に変換してダウンロード（拡張子 `.pdf`） |
| `Dispose()` | `void` | リソース解放（`using` 推奨） |

`ExcelCellIndex` のプロパティ:

| プロパティ | 型 | 説明 |
|---|---|---|
| `RowIndex` | `int` | 行インデックス（1 始まり） |
| `ColumnIndex` | `int` | 列インデックス（1 始まり） |

`GetNext(int rowOffset, int columnOffset)` で相対位置のセルを取得できます。

```csharp
using var stream = await Resources.GetMemoryStream("Templates/Invoice.xlsx");
using var excel = new Excel(stream, "請求書.xlsx");

await excel.OverWrite(this);          // {NameField} 等のプレースホルダを置換
var cell = excel.FindCellByText("[小計]");
if (cell != null) excel.SetCellValue(cell, 12345);

await excel.DownloadPdf();
```

詳細は [チュートリアル: Excel 帳票と PDF 出力](../tutorials/tutorial_excel_pdf.md) を参照。

---

## JsonObject — JSON の動的操作

`JsonObject` は型扱いです（サービスではない）。`new JsonObject()` で作成して、辞書 / 配列の両方として使えます。

#### インスタンスメソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `obj["key"]` | `object?` | 辞書アクセス（取得時に存在しなければ空の `JsonObject` を作って返す） |
| `obj[index]` | `object?` | 配列アクセス |
| `Count` | `int` | 要素数 |
| `Add(object?)` | `void` | 配列に追加 |
| `ToJsonString()` | `string` | JSON 文字列化 |

#### 静的メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `JsonObject.SerializeObject(object obj)` | `JsonObject` | 任意のオブジェクトを `JsonObject` 化 |
| `JsonObject.ToJsonObject(string text)` | `JsonObject` | JSON 文字列をパース |
| `JsonObject.ToJsonString(JsonObject obj)` | `string` | JSON 文字列化 |

```csharp
var body = new JsonObject();
body["name"] = "山田";
body["age"] = 30;

var result = await WebApiService.Post("/api/users", body);
foreach (var item in result.JsonObject)
{
    Logger.Log(item.id + ": " + item.name);
}
```

---

## 関連項目

- [スクリプト概要](script.md)
- [スクリプト構文リファレンス](script_syntax.md)
- [ModuleSearcher / BatchSearcher](script_module_searcher.md)
- [スクリプトの拡張](script_extend.md)
- [チュートリアル: WebAPI 連携](../tutorials/tutorial_webapi.md)
- [チュートリアル: Excel 帳票と PDF 出力](../tutorials/tutorial_excel_pdf.md)
- [Tips: メールを送信する](../Examples/SendingMail.md)
