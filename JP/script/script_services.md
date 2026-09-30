# 組み込みサービスとテンプレート由来サービス

スクリプトから呼び出せるサービスは、提供元によって 2 種類に分かれます。

| 区分 | 提供元 | カスタマイズ |
|---|---|---|
| **組み込みサービス** | `Codeer.LowCode.Blazor` 本体 | 不可（言語仕様の一部） |
| **テンプレート由来サービス** | アプリテンプレートの `LowCodeApp.Client.Shared` が登録するもの（大半は拡張ライブラリ Extras が提供） | **登録の追加・削除・差し替えが可能** |

テンプレート由来サービスは `AppInfoService` のコンストラクタで登録されています（Extras のものは `ExtrasClientInitializer.Initialize(...)` がまとめて登録）。
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
Logger.Log("デバッグ情報: " + Name.Value);
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
var result = MessageBox.Show("削除しますか？", "はい", "いいえ");
if (result == "はい") { ... }

MessageBox.Show("注意",
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
| `NavigationService.SetRecordPagingContext(...)` | `void` | 詳細画面の [レコードページ送り](../fields/RecordPaging.md#スクリプトで組んだ一覧から遷移する場合) に一覧の並びを登録する |

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
using var stream = Resources.GetMemoryStream("Templates/Invoice.xlsx");
var template = Resources.GetText("Templates/MailBody.txt");
```

### BatchSearcher — 複数検索を 1 回のリクエストで実行

`ModuleSearcher` を複数まとめて投げて、サーバーラウンドトリップを節約します。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `BatchSearcher.Execute(params ModuleSearcher[])` | `Task<BatchModuleSearchResponse>` | 複数検索を一括実行 |

```csharp
var s1 = new ModuleSearcher<Customer>();
var s2 = new ModuleSearcher<Order>();
var response = BatchSearcher.Execute(s1, s2);

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

アプリテンプレートで作成したプロジェクトでは、`LowCodeApp.Client.Shared/Services/AppInfoService.cs` のコンストラクタで次のサービス・型がスクリプトに登録されています。
大半は拡張ライブラリ [Codeer.LowCode.Blazor.Extras](https://github.com/Codeer-Software/Codeer.LowCode.Blazor.Extras) が提供し、`ExtrasClientInitializer.Initialize(...)` の 1 行でまとめて登録されます。

| サービス / 型 | 提供元 | 用途 |
|---|---|---|
| `LoadingService` | 本体（テンプレートが登録） | 処理中インジケータの表示 |
| `Toaster` | Extras | トースト通知 |
| `WebApiService` / `WebApiResult` | Extras | 外部 API・追加 Controller の HTTP 呼び出し |
| `Excel` / `ExcelCellIndex` | Extras | Excel テンプレートによる帳票作成と xlsx / PDF ダウンロード |
| `BulkFileReader<モジュール>` | Extras | CSV / 固定長 / Excel ファイルの取込（スクリプトで加工してから保存する場合） |
| `BulkFileTransferService` | Extras | 一括ダウンロードと一括保存 |

Extras はソースを MIT で公開しているので、動作を変えたい場合はソースをコピーして改変したクラスを代わりに登録できます。独自のサービスを追加する方法は [スクリプトの拡張](script_extend.md) を参照してください。
各オブジェクトの正確なシグネチャは、デザイナのスクリプトエディタの入力補完で確認できます。

> 以前のテンプレートにあった `MailService`（メール送信）は削除されました。メールは Extras の MailField で送ります（[メールを送信する](../Examples/SendingMail.md)）。

### LoadingService — 処理中インジケータ

`StartLoading` で返るスコープを `using` で囲んだ間、画面に処理中インジケータを表示します。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `LoadingService.StartLoading(int? delayTime)` | `LoadingScope` | インジケータ表示を開始する。`delayTime`（ミリ秒）を指定すると、その時間より長くかかったときだけ表示する |

```csharp
void SaveButton_OnClick()
{
    using var loading = LoadingService.StartLoading(1000);   // 1 秒以上かかるときだけ表示
    var ret = this.Submit();
    if (ret != true) Toaster.Error("保存に失敗しました");
}
```

### Toaster — トースト通知

画面右下に一時的なメッセージを出します（非ブロッキング）。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `Toaster.Success(string)` | `void` | 成功通知（表示中のトーストを消してから表示） |
| `Toaster.Info(string)` | `void` | 情報通知 |
| `Toaster.Warn(string)` | `void` | 警告通知 |
| `Toaster.Error(string)` | `void` | エラー通知（見逃さないよう長めに表示） |

```csharp
Toaster.Success("保存しました");
Toaster.Error("入力エラーがあります");
```

### WebApiService — HTTP 呼び出し

外部 API や同一プロジェクトの追加 Controller を呼び出します。

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `WebApiService.Get(string url)` | `WebApiResult` | GET |
| `WebApiService.Post(string url, JsonObject data)` | `WebApiResult` | POST |
| `WebApiService.Put(string url, JsonObject data)` | `WebApiResult` | PUT |
| `WebApiService.Delete(string url)` | `WebApiResult` | DELETE |

`WebApiResult` のプロパティ:

| プロパティ | 型 | 説明 |
|---|---|---|
| `StatusCode` | `int` | HTTP ステータス |
| `JsonObject` | `JsonObject` | レスポンス本文を JSON として保持 |

```csharp
var result = WebApiService.Get("/testapi/weather");
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

### Excel — Excel テンプレートで帳票を作って Excel または PDF で配布

帳票テンプレート（`.xlsx`）を読み込み、Module の値で穴埋めしてダウンロードさせる仕組みです。

#### 生成

```csharp
using var stream = Resources.GetMemoryStream("Templates/Invoice.xlsx");
using var excel = new Excel(stream, "請求書.xlsx");
```

#### メソッド

| メソッド | 戻り値 | 説明 |
|---|---|---|
| `OverWrite(Module data)` | `void` | セルの内容全体が `$` で始まるセル（例: `$Name.Value`）を、Module から辿った値で置換 |
| `FindCellByText(string text)` | `ExcelCellIndex?` | 指定テキストに一致するセルを検索 |
| `SetCellValue(ExcelCellIndex cell, object value)` | `void` | セルに値をセット |
| `CopyCells(ExcelCellIndex source, ExcelCellIndex destination, int rowCount, int colCount)` | `void` | セル範囲をコピー |
| `AddImage(ExcelCellIndex cellIndex, Stream stream)` | `void` | 画像を貼り付け |
| `Download()` | `bool` | Excel 形式でダウンロード（拡張子 `.xlsx`） |
| `DownloadPdf()` | `bool` | PDF に変換してダウンロード（拡張子 `.pdf`） |
| `Dispose()` | `void` | リソース解放（`using` 推奨） |

`ExcelCellIndex` のプロパティ:

| プロパティ | 型 | 説明 |
|---|---|---|
| `RowIndex` | `int` | 行インデックス（1 始まり） |
| `ColumnIndex` | `int` | 列インデックス（1 始まり） |

`GetNext(int rowOffset, int columnOffset)` で相対位置のセルを取得できます。

`OverWrite` のテンプレートの書き方:

- 置換はセル単位。**セルの内容全体**が `$` で始まるとき、`$` の後ろを Module からの参照（スクリプトで `this` から辿るのと同じ名前。`フィールド名.Value` が基本形）として解決し、そのセルを値で置き換える（例: `$Title.Value` / `$OrderDate.Value` / `$Customer.DisplayText`）
- 数値・日付は値のまま書き込まれる。桁区切りや日付の書式はテンプレートのセルの表示形式で決める
- 解決できない参照のセルはそのまま残る
- 「見積番号: $Id.Value」のように文字と混ぜたセルは置き換わらない。ラベルと値はセルを分ける

```csharp
using var stream = Resources.GetMemoryStream("Templates/Invoice.xlsx");
using var excel = new Excel(stream, "請求書.xlsx");

excel.OverWrite(this);          // $Name.Value などと書いたセルを値で置換
var cell = excel.FindCellByText("[小計]");
if (cell != null) excel.SetCellValue(cell, 12345);

excel.DownloadPdf();
```

詳細は [チュートリアル: Excel 帳票と PDF 出力](../tutorials/tutorial_excel_pdf.md) を参照。

### BulkFileReader / BulkFileTransferService — ファイルの一括取込・出力

一覧ページの一括ダウンロード / 一括更新と同じ形式（Excel、またはモジュールに CSV・固定長の定義があればその形式）で、スクリプトからファイルを入出力します。
取込のたびにコード変換・検証・行の除外などを挟みたい場合に使います。

| メンバー | 説明 |
|---|---|
| `new BulkFileReader<モジュール>()` | 取込先モジュールを指定して作る（一度変数に受けてから使う） |
| `reader.Read()` | ファイル選択ダイアログを開き、選ばれたファイルを解析する。キャンセルなら `false`。DB には書き込まない |
| `reader.Items` | 解析した行（ファイルの行順）。値の参照・書き換えができる |
| `reader.HasError` / `reader.ErrorText` / `reader.DownloadErrorText()` | 解釈できなかったセルの有無 / 詳細テキスト / 詳細をファイルでダウンロード |
| `BulkFileTransferService.Submit(行のリスト)` | 行をまとめて 1 トランザクションで保存する。Id の有無で追加 / 更新を判定。保存した新規行の Id は返らない |
| `BulkFileTransferService.Download(...)` | ModuleSearcher・検索フィールド・リストフィールドの条件、または行のリストをファイルでダウンロードする |

```csharp
void Import_OnClick()
{
    var reader = new BulkFileReader<注文>();
    if (!reader.Read()) return;           // キャンセル
    if (reader.HasError)
    {
        reader.DownloadErrorText();
        return;
    }
    if (BulkFileTransferService.Submit(reader.Items)) Toaster.Success("取り込みました");
}
```

取込ボタンは DB に結びつかない画面専用のモジュールに置くのが基本です。形式の定義と使い分けは [取込書出パターン](../patterns/import_export.md) を参照してください。

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

var result = WebApiService.Post("/api/users", body);
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
