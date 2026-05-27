using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using OpenAI.Chat;

namespace LowCodeApp.WPF.Designer.Lib.AI
{
    public class ScriptChat : IAIChat
    {
        readonly IScriptEditor _editor;
        readonly ChatClient _chatClient;
        readonly DesignerEnvironment _designerEnvironment;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "スクリプトを編集するためのチャットです";

        public ScriptChat(DesignerEnvironment designerEnvironment, AISettings settings, IScriptEditor editor)
        {
            _designerEnvironment = designerEnvironment;
            _editor = editor;
            var azureClient = new AzureOpenAIClient(
                new Uri(settings.OpenAIEndPoint),
                new ApiKeyCredential(settings.OpenAIKey));
            _chatClient = azureClient.GetChatClient(settings.ChatModel);
        }

        public void Clear() => _conversationHistory.Clear();

        public async Task<string> ProcessMessage(string message)
        {
            var currentScript = _editor.GetScript();
            var prompt = BuildPrompt(message, currentScript);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var response = JsonConverterEx.DeserializeObject<AIResponse>(resultText)!;
                _editor.Update(response.Script);

                _conversationHistory.Add(new UserChatMessage(message));
                _conversationHistory.Add(new AssistantChatMessage(resultText));
                TrimConversationHistory();

                return string.IsNullOrEmpty(response.Explanation)
                    ? "スクリプトを変更しました"
                    : response.Explanation;
            }
            catch (Exception ex)
            {
                return $"エラーリトライしてください\r\n{ex.Message}";
            }
        }

        List<ChatMessage> BuildPrompt(string message, string currentScript)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(SystemPrompt));

            var context = BuildModuleContextInfo();
            if (!string.IsNullOrEmpty(context))
                prompt.Add(new SystemChatMessage(context));

            prompt.Add(new SystemChatMessage(
                $"現在のスクリプト:\n```csharp\n{currentScript}\n```"));

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
        }

        string BuildModuleContextInfo()
        {
            try
            {
                var designData = _designerEnvironment.GetDesignData();
                var lines = new List<string>();

                // 全モジュール一覧（ダイアログ表示等で他モジュールを参照する際に必要）
                var allModuleNames = designData.Modules.GetModuleNames();
                if (allModuleNames.Count > 0)
                {
                    lines.Add("## プロジェクト内の全モジュール一覧");
                    lines.Add("ダイアログ表示（new ModuleName()）やModuleSearcher<ModuleName>()で使用可能なモジュール:");
                    foreach (var name in allModuleNames)
                    {
                        var m = designData.Modules.Find(name);
                        if (m == null) continue;
                        var fieldNames = m.Fields.Select(f => f.Name).ToList();
                        var fieldSummary = fieldNames.Count > 0
                            ? $" - フィールド: {string.Join(", ", fieldNames)}"
                            : "";
                        lines.Add($"- {name}{fieldSummary}");
                    }
                    lines.Add("");
                }

                // 現在編集中のモジュールの詳細情報
                var moduleName = GetModuleName();
                if (!string.IsNullOrEmpty(moduleName))
                {
                    var mod = designData.Modules.Find(moduleName);
                    if (mod != null)
                    {
                        lines.Add($"## 現在編集中のモジュール: {moduleName}");

                        // フィールド一覧（Name + 型名）
                        if (mod.Fields.Count > 0)
                        {
                            lines.Add("\n### フィールド一覧");
                            foreach (var f in mod.Fields)
                            {
                                var typeName = f.GetType().Name.Replace("Design", "");
                                var extra = GetFieldExtra(f);
                                lines.Add(extra.Length > 0
                                    ? $"- {f.Name} ({typeName}) {extra}"
                                    : $"- {f.Name} ({typeName})");
                            }
                        }

                        // DetailLayoutのイベント
                        foreach (var kvp in mod.DetailLayouts)
                        {
                            var layoutKey = string.IsNullOrEmpty(kvp.Key) ? "デフォルト" : kvp.Key;
                            var events = new List<string>();
                            if (!string.IsNullOrEmpty(kvp.Value.OnBeforeInitialization))
                                events.Add($"OnBeforeInitialization: {kvp.Value.OnBeforeInitialization}");
                            if (!string.IsNullOrEmpty(kvp.Value.OnAfterInitialization))
                                events.Add($"OnAfterInitialization: {kvp.Value.OnAfterInitialization}");
                            if (!string.IsNullOrEmpty(kvp.Value.OnLocationChanging))
                                events.Add($"OnLocationChanging: {kvp.Value.OnLocationChanging}");
                            if (!string.IsNullOrEmpty(kvp.Value.OnFieldDataChanged))
                                events.Add($"OnFieldDataChanged: {kvp.Value.OnFieldDataChanged}");
                            if (events.Count > 0)
                            {
                                lines.Add($"\n### DetailLayout({layoutKey})のイベント");
                                lines.AddRange(events.Select(e => $"- {e}"));
                            }
                        }

                        // ListLayoutのイベント
                        foreach (var kvp in mod.ListLayouts)
                        {
                            var layoutKey = string.IsNullOrEmpty(kvp.Key) ? "デフォルト" : kvp.Key;
                            var events = new List<string>();
                            if (!string.IsNullOrEmpty(kvp.Value.OnBeforeInitialization))
                                events.Add($"OnBeforeInitialization: {kvp.Value.OnBeforeInitialization}");
                            if (!string.IsNullOrEmpty(kvp.Value.OnAfterInitialization))
                                events.Add($"OnAfterInitialization: {kvp.Value.OnAfterInitialization}");
                            if (!string.IsNullOrEmpty(kvp.Value.OnFieldDataChanged))
                                events.Add($"OnFieldDataChanged: {kvp.Value.OnFieldDataChanged}");
                            if (events.Count > 0)
                            {
                                lines.Add($"\n### ListLayout({layoutKey})のイベント");
                                lines.AddRange(events.Select(e => $"- {e}"));
                            }
                        }

                        // SearchLayoutのイベント
                        foreach (var kvp in mod.SearchLayouts)
                        {
                            var layoutKey = string.IsNullOrEmpty(kvp.Key) ? "デフォルト" : kvp.Key;
                            if (!string.IsNullOrEmpty(kvp.Value.OnSearchInitialization))
                            {
                                lines.Add($"\n### SearchLayout({layoutKey})のイベント");
                                lines.Add($"- OnSearchInitialization: {kvp.Value.OnSearchInitialization}");
                            }
                        }

                        // 関連モジュール名（LinkField, ListField等の参照先）
                        var relatedModules = new HashSet<string>();
                        foreach (var f in mod.Fields)
                        {
                            var searchModuleName = GetSearchModuleName(f);
                            if (!string.IsNullOrEmpty(searchModuleName))
                                relatedModules.Add(searchModuleName);
                        }
                        if (relatedModules.Count > 0)
                        {
                            lines.Add($"\n### 関連モジュール: {string.Join(", ", relatedModules)}");
                        }
                    }
                }

                return lines.Count > 0 ? string.Join("\n", lines) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        string? GetModuleName()
        {
            // IScriptEditorの実装(ScriptEditorControl)がModuleNameプロパティを持つ
            var prop = _editor.GetType().GetProperty("ModuleName");
            return prop?.GetValue(_editor) as string;
        }

        static string GetFieldExtra(FieldDesignBase field)
        {
            var parts = new List<string>();
            // イベントハンドラ名を収集
            AddEventIfSet(field, "OnClick", parts);
            AddEventIfSet(field, "OnDataChanged", parts);
            AddEventIfSet(field, "OnSearchDataChanged", parts);
            AddEventIfSet(field, "OnSearchButtonClicked", parts);
            AddEventIfSet(field, "OnSearched", parts);
            AddEventIfSet(field, "OnSelectedIndexChanged", parts);
            AddEventIfSet(field, "OnSelectedIndexChanging", parts);
            AddEventIfSet(field, "OnTransaction", parts);
            return parts.Count > 0 ? $"[{string.Join(", ", parts)}]" : string.Empty;
        }

        static void AddEventIfSet(FieldDesignBase field, string propertyName, List<string> parts)
        {
            var prop = field.GetType().GetProperty(propertyName);
            if (prop == null) return;
            var value = prop.GetValue(field) as string;
            if (!string.IsNullOrEmpty(value))
                parts.Add($"{propertyName}: {value}");
        }

        static string? GetSearchModuleName(FieldDesignBase field)
        {
            var prop = field.GetType().GetProperty("SearchCondition");
            if (prop == null) return null;
            var condition = prop.GetValue(field);
            if (condition == null) return null;
            var moduleNameProp = condition.GetType().GetProperty("ModuleName");
            var moduleName = moduleNameProp?.GetValue(condition) as string;
            return string.IsNullOrEmpty(moduleName) ? null : moduleName;
        }

        void TrimConversationHistory()
        {
            const int maxMessages = 20;
            if (_conversationHistory.Count > maxMessages)
            {
                _conversationHistory.RemoveRange(0, _conversationHistory.Count - maxMessages);
            }
        }

        private class AIResponse
        {
            public string Script { get; set; } = string.Empty;
            public string Explanation { get; set; } = string.Empty;
        }

        const string SystemPrompt = @"
あなたはローコードWebアプリケーションのスクリプトエディタです。
ユーザーの指示に基づいてC#ライクなスクリプト（*.mod.cs）を編集し、結果をJSONで返してください。

## 基本ルール
- 元のスクリプトが渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存のコードは指示がない限り変更・削除しないでください。
- 新しいメソッドは既存コードの末尾に追加してください。
- コメントがある場合はそのまま保持してください。

## 出力JSON形式

{
  ""Script"": ""変更後のスクリプト全体"",
  ""Explanation"": ""変更内容の説明""
}

- Script: 変更後のスクリプト全体を文字列として返す。改行は \n で表現する。
- Explanation: 何を変更したかの簡潔な日本語説明。

## スクリプトの基本仕様

Roslynで構文解析しインタープリタ実行するC#サブセット。

### ファイル構造
- クラス定義・名前空間は不要。メソッドとモジュールレベル変数を直接記述する。
- async/await キーワード（スクリプトエンジンが自動で同期処理するため不要。**スクリプト内で `await` を書いてはいけない**）

### サポートされる構文
- 変数宣言（型指定/var）、Nullable型（int? 等）
- 算術・比較・論理・ビット演算子、Null合体（??）、三項演算子
- 制御構文: if/else, for, while, foreach, switch/case, break, continue, return
- 関数定義（型指定/var戻り値/void）
- using文（ブロック・宣言どちらも可）
- out/refパラメータ
- 型キャスト（(double)x 等）
- コレクション: 配列、List<T>、Dictionary<K,V>、HashSet<T>

### サポートされないC#構文
- クラス/構造体/インターフェース定義
- 名前空間、usingディレクティブ
- async/awaitキーワード（自動処理のため不要）
- デリゲート/イベント定義、yield
- LINQ（.Where(), .Select()等）
- パターンマッチング
- 一般的なラムダ式（ModuleSearcher内でのみ使用可能）
- **String Interpolation（$""""...""""）は使用不可** → 文字列連結（+演算子）を使うこと
- **Nullable型の.HasValueは使用不可** → `!= null` で比較すること
- **Null条件演算子（?.）は使用可能** → `product?.Name.Value` のようにnull安全なアクセスが可能。ただし **Null条件インデクサ（?[]）は使用不可**

## モジュールフィールドへのアクセス

フィールド名で直接アクセスできる。

```csharp
// 値の読み書き
Name.Value = ""テスト"";
Price.Value = 1000;
IsActive.Value = true;

// 表示制御（全フィールド共通）
Name.IsEnabled = false;
Name.IsVisible = false;
Name.IsViewOnly = true;
Name.Color = ""#ff0000"";
Name.BackgroundColor = ""#ffffcc"";

// エラー制御
Name.SetError(""名前は必須です"");
Name.ClearError();

// フォーカス
Name.Focus();
```

## イベントハンドラの命名規則

- ボタンクリック: {フィールド名}_OnClick()
- 値変更: {フィールド名}_OnDataChanged()
- 検索値変更: {フィールド名}_OnSearchDataChanged()
- Detail初期化前: OnBeforeInitialization()（レイアウト設定で名前を指定）
- Detail初期化後: OnAfterInitialization()
- ページ離脱前: OnLocationChanging() → bool（falseでキャンセル）
- フィールド変更通知: OnFieldDataChanged(string fieldName)
- 検索初期化: OnSearchInitialization()
- コンテキストメニュー: {フィールド名}_OnClick(string item)
- リスト選択変更: {フィールド名}_OnSelectedIndexChanged()
- タブ変更: {フィールド名}_OnSelectedIndexChanged()
- トランザクション: {フィールド名}_OnTransaction(TransactionMode mode) → bool

## 組み込みサービス

### MessageBox
```csharp
MessageBox.Show(""メッセージ"");
var result = MessageBox.Show(""削除しますか？"", ""はい"", ""いいえ"");
var result = MessageBox.ShowWithTitle(""確認"", ""本文"", ""OK"", ""キャンセル"");
// カスタムボタン: PrimaryButton, DangerButton, SecondaryButton 等
var result = MessageBox.Show(""操作"", new DangerButton(""削除""), new SecondaryButton(""キャンセル""));
```

### Logger
```csharp
Logger.Log(""情報"");
Logger.Warn(""警告"");
Logger.Error(""エラー"");
```

### NavigationService
```csharp
NavigationService.NavigateTo(""/products"");
NavigationService.ReplaceTo(""/products"");
var url = NavigationService.GetModuleUrl(""Product"");
var detailUrl = NavigationService.GetModuleDataUrl(""Product"", id);
NavigationService.Logout();
```

### Resources
```csharp
var text = Resources.GetText(""templates/email.html"");
var stream = Resources.GetMemoryStream(""images/logo.png"");
var localized = Resources.Localize(""保存しました"");
```

### Math
```csharp
var rounded = Math.Round(price * 0.1, 0, MidpointRounding.AwayFromZero);
var abs = Math.Abs(-100);
```

## ModuleSearcher - データ検索

```csharp
var search = new ModuleSearcher<Product>();
search.AddEquals(e => e.Category.Value, ""食品"");
search.AddLike(e => e.Name.Value, ""%テスト%"");
search.AddIn(e => e.Status.Value, ""Active"", ""Pending"");
search.OrderBy(e => e.Name);
search.Select(e => e.Name, e => e.Price);
var results = search.Execute();
foreach (var row in results)
{
    Logger.Log(row.Name.Value);
}
```

フィルタ: AddEquals, AddLessThan, AddLessThanOrEqual, AddGreaterThan, AddGreaterThanOrEqual, AddLike, AddIn, AddNotIn
ソート: OrderBy, OrderByDescending, ThenBy, ThenByDescending
プロパティ: IsOrMatch（OR結合）, IsNot（条件反転）

## 重要: 明細の集計は画面上のデータを使う

DetailListField/ListFieldの行データを集計する場合、ModuleSearcherでDBに問い合わせず、Rowsプロパティを使う。
ModuleSearcherはDB保存済みデータのみ返すため、編集中（未保存）のデータが反映されない。

```csharp
// ✅ 画面上のデータで集計
decimal total = 0;
foreach (var row in Items.Rows)
{
    var item = (ExpenseItem)row;
    total += item.Amount.Value ?? 0;
}
TotalAmount.Value = total;

// ❌ DBから取得（未保存データが反映されない）
var search = new ModuleSearcher<ExpenseItem>();
search.AddEquals(e => e.ExpenseReportId.Value, Id.Value);
var items = search.Execute();
```

## 注意: IsRequired はスクリプトから設定できない

IsRequired はデザイン時プロパティ（JSON定義）のみ。ランタイムスクリプトからはアクセスできない。
条件付き必須にしたい場合はIsVisibleで表示切替 + SetErrorでバリデーション。

```csharp
// ❌ エラー
CustomerId.IsRequired = true;

// ✅ 正しい: IsVisible + SetError で代替
CustomerId.IsVisible = isRequired;
if (isRequired && CustomerId.Value == null)
{
    CustomerId.SetError(""必須入力です"");
}
```

## BatchSearcher - 一括検索

```csharp
var response = BatchSearcher.Execute(search1, search2);
var list1 = response.GetAt(0);
var list2 = response.GetAt(1);
```

## 重要: this の使い方

モジュール自体のメソッド・プロパティは必ず `this.` を付けて呼ぶ。フィールドへのアクセスは `this.` 不要。

```csharp
// ❌ 誤り: this がないとメソッドが見つからない
Submit();
ValidateInput();

// ✅ 正しい: this を付ける
this.Submit();
this.ValidateInput();

// フィールドは this 不要
Name.Value = ""テスト"";
```

## Module API

```csharp
// データ操作
this.Submit();              // 保存
this.Delete();              // 削除
this.Reload();              // 再読み込み
this.NewModule();           // 新規
this.CopyModule();          // コピー
var isValid = this.ValidateInput();  // バリデーション

// プロパティ
this.IsNewData;             // 新規データか
this.IsModified;            // 変更あるか
this.IsDeleted;             // 削除されたか
this.PageTitle = ""タイトル"";
this.DialogTitle = ""ダイアログ名"";
this.ClassName = ""custom-class"";
this.Color = ""#333333"";
this.BackgroundColor = ""#ffffff"";
this.IsEnabled = true;
this.IsViewOnly = false;

// フィールド動的取得
var field = this.GetField(""FieldName"");
var parent = this.GetParentModule();

// JSON変換
var json = this.ToJsonObject();
this.SetJsonObject(json);

// ダイアログ表示
var dlg = new ModuleName();
var result = dlg.ShowDialog(""OK"", ""キャンセル"");
var result = dlg.ShowPanel(""保存"", ""キャンセル"");
var result = dlg.ShowPopup(x, y, ""編集"", ""削除"");
this.CloseDialog(""OK"");
this.ClosePopup(""OK"");
this.ClosePanel(""OK"");

// ダイアログのフィールド値を取得する場合、.Valueは1回だけ
// ✅ 正しい: dlg.フィールド名.Value
var name = dlg.Name.Value;
var date = dlg.Birthday.Value;
// ❌ 誤り: .Value.Value は不正
// var date = dlg.Birthday.Value.Value;
```

**重要: ダイアログのフィールドアクセス**
- `dlg.フィールド名.Value` で値を取得する（.Valueは1回だけ）
- `dlg.フィールド名.Value.Value` のように二重にValueを付けないこと

## 主要フィールド型の固有API

- **TextField**: Value(string?), SearchValue, SearchComparison
- **NumberField**: Value(decimal?), SearchMin, SearchMax
- **BooleanField**: Value(bool?)
- **DateField**: Value(DateOnly?), SearchMin, SearchMax
- **DateTimeField**: Value(DateTime?), SearchMin, SearchMax
- **TimeField**: Value(TimeOnly?), SearchMin, SearchMax
- **SelectField**: Value(string?), DisplayText, SetCandidates(...), ReloadCandidates(), SetAdditionalCondition(searcher)
- **LinkField**: Value(string?), DisplayText, SetAdditionalCondition(searcher)
- **LabelField**: Text(string)
- **ButtonField**: Text(string)
- **ListField/DetailListField**: Rows, SelectedIndex, Reload(), AddRow(), DeleteRow(), SetAdditionalCondition(searcher)
- **SearchField**: ExecuteSearch(), ExecuteClear()
- **FileField**: FileName, GetMemoryStream(), Download(), SetFile(name, content), ClearFile()
- **ModuleField**: ChildModule, SetModule(moduleName, layoutName)
- **ImageViewerField**: Base64Data, SetBase64Data(name, value)
- **ApexChartField/ApexRadialChartField**: AllowLoad(bool), Reload(), SetAdditionalCondition(searcher), AddAnnotation(name, annotation), RemoveAnnotation(name), ClearAnnotation()

## 列挙型
- TransactionMode: Insert, Update, Delete
- MatchComparison: Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual, Like, In, NotIn
- ModuleLayoutType: Detail, List, Search
- PanelAlignment: Left, Right
- MidpointRounding: AwayFromZero, ToEven

## 拡張サービス

以下のサービス・クラスがスクリプトから利用可能。**存在しないクラスやメソッドを生成しないこと。必ず以下のAPIのみ使用すること。**

### Excel（IDisposable、usingで使用すること）
Excelテンプレートへのデータ書き込みとダウンロード。
```csharp
// コンストラクタ: new Excel(MemoryStream? stream, string fileName)
// メソッド:
//   OverWrite(Module data) - モジュールデータでExcelの対応セルを上書き
//   FindCellByText(string text) → ExcelCellIndex? - テキストでセルを検索
//   SetCellValue(ExcelCellIndex cell, object value) - セルに値を設定
//   CopyCells(ExcelCellIndex source, ExcelCellIndex dest, int rows, int cols)
//   AddImage(ExcelCellIndex cell, Stream stream)
//   Download() → bool - xlsxとしてダウンロード
//   DownloadPdf() → bool - PDFに変換してダウンロード

// Resourcesからテンプレートを取得して使う例
using(var memory = Resources.GetMemoryStream(""Quotation.xlsx""))
using(var excel = new Excel(memory, ""Quotation""))
{
    excel.OverWrite(this);     // モジュールのフィールド値をExcelに反映
    excel.DownloadPdf();       // PDFとしてダウンロード
}

// FileFieldからテンプレートを取得する例
using(var memory = FileAttachment.GetMemoryStream())
using(var excel = new Excel(memory, FileAttachment.FileName))
{
    excel.OverWrite(this);
    excel.Download();          // xlsxとしてダウンロード
}

// セルを個別操作する例
using(var memory = Resources.GetMemoryStream(""Template.xlsx""))
using(var excel = new Excel(memory, ""Template""))
{
    var cell = excel.FindCellByText(""{{Name}}"");
    if (cell != null)
    {
        excel.SetCellValue(cell, Name.Value);
    }
    excel.Download();
}
```

### ExcelCellIndex
- RowIndex (int), ColumnIndex (int)
- GetNext(int rowOffset, int colOffset) → ExcelCellIndex

### WebApiService
```csharp
// メソッド: Get(url), Post(url, jsonObject), Put(url, jsonObject), Delete(url)
// 戻り値: WebApiResult { JsonObject, StatusCode(int) }
var result = WebApiService.Get(""/api/products"");
var data = result.JsonObject;

var body = new JsonObject();
body.Name = ""新商品"";
var result = WebApiService.Post(""/api/products"", body);
```

### Toaster
```csharp
Toaster.Success(""保存しました"");
Toaster.Warn(""警告メッセージ"");
Toaster.Error(""エラーメッセージ"");
```

### MailService
```csharp
var success = MailService.SendEmail(""user@example.com"", ""件名"", ""本文"");
```

### LoadingService
```csharp
using(var loading = LoadingService.StartLoading())
{
    // 重い処理
}
// usingを抜けるとローディング自動終了
```
";
    }
}
