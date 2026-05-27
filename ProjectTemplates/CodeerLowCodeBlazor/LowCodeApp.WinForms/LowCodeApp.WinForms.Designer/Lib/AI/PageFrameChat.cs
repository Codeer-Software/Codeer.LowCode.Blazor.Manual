using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using OpenAI.Chat;

namespace LowCodeApp.WinForms.Designer.Lib.AI
{
    public class PageFrameChat : IAIChat
    {
        readonly IPageFrameEditor _editor;
        readonly ChatClient _chatClient;
        readonly DesignerEnvironment _designerEnvironment;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "ページフレームを編集するためのチャットです";

        public PageFrameChat(DesignerEnvironment designerEnvironment, AISettings settings, IPageFrameEditor editor)
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
            var pageFrame = _editor.GetPageFrameDesign();

            var prompt = BuildPrompt(message, pageFrame);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var response = JsonConverterEx.DeserializeObject<AIResponse>(resultText)!;
                ApplyResponse(pageFrame, response);

                _conversationHistory.Add(new UserChatMessage(message));
                _conversationHistory.Add(new AssistantChatMessage(resultText));
                TrimConversationHistory();

                return string.IsNullOrEmpty(response.Explanation)
                    ? "変更しました"
                    : response.Explanation;
            }
            catch (Exception ex)
            {
                return $"エラーリトライしてください\r\n{ex.Message}";
            }
        }

        List<ChatMessage> BuildPrompt(string message, PageFrameDesign pageFrame)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(BuildSystemPrompt()));
            prompt.Add(new SystemChatMessage(BuildDesignContextInfo()));
            prompt.Add(new SystemChatMessage(
                $"現在のページフレーム定義:\n{JsonConverterEx.SerializeObject(pageFrame)}"));

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
        }

        string BuildDesignContextInfo()
        {
            var lines = new List<string> { "## 現在のアプリケーション情報" };

            try
            {
                var designData = _designerEnvironment.GetDesignData();

                var moduleNames = designData.Modules.GetModuleNames();
                if (moduleNames.Any())
                {
                    lines.Add("\n### モジュール一覧（PageLinkのModuleに指定可能）");
                    foreach (var name in moduleNames)
                    {
                        var mod = designData.Modules.Find(name);
                        if (mod == null) continue;

                        var details = new List<string>();
                        if (mod.DetailLayouts.Any())
                            details.Add($"DetailLayout: {string.Join(", ", mod.DetailLayouts.Keys.Select(k => string.IsNullOrEmpty(k) ? "(default)" : k))}");
                        if (mod.ListLayouts.Any())
                            details.Add($"ListLayout: {string.Join(", ", mod.ListLayouts.Keys.Select(k => string.IsNullOrEmpty(k) ? "(default)" : k))}");
                        if (mod.SearchLayouts.Any())
                            details.Add($"SearchLayout: {string.Join(", ", mod.SearchLayouts.Keys.Select(k => string.IsNullOrEmpty(k) ? "(default)" : k))}");

                        lines.Add($"- {name}");
                        if (details.Any())
                            lines.Add($"  {string.Join(", ", details)}");
                    }
                }

                var pageFrameNames = designData.PageFrames.GetPageFrameNames();
                if (pageFrameNames.Any())
                {
                    lines.Add("\n### ページフレーム一覧（PageLinkのPageFrameに指定可能）");
                    foreach (var name in pageFrameNames)
                    {
                        lines.Add($"- {name}");
                    }
                }
            }
            catch
            {
                lines.Add("（デザインデータの取得に失敗しました）");
            }

            return string.Join("\n", lines);
        }

        void ApplyResponse(PageFrameDesign pageFrame, AIResponse response)
        {
            pageFrame.IsApplicationRoot = response.PageFrame.IsApplicationRoot;
            pageFrame.Description = response.PageFrame.Description;
            pageFrame.Left = response.PageFrame.Left;
            pageFrame.Right = response.PageFrame.Right;
            pageFrame.Header = response.PageFrame.Header;
            pageFrame.Padding = response.PageFrame.Padding;
            pageFrame.BackgroundColor = response.PageFrame.BackgroundColor;
            pageFrame.Color = response.PageFrame.Color;
            pageFrame.FontFamily = response.PageFrame.FontFamily;
            pageFrame.FontSize = response.PageFrame.FontSize;
            pageFrame.TopPageModuleDesign = response.PageFrame.TopPageModuleDesign;
            pageFrame.OtherPageModuleDesigns = response.PageFrame.OtherPageModuleDesigns;
            _editor.Update();
        }

        void TrimConversationHistory()
        {
            const int maxMessages = 20;
            if (_conversationHistory.Count > maxMessages)
            {
                _conversationHistory.RemoveRange(0, _conversationHistory.Count - maxMessages);
            }
        }

        string BuildSystemPrompt()
        {
            return SystemPrompt + $@"

## TypeFullName一覧（ListFieldDesignのJSONに必ず正確に設定すること）
- ListFieldDesign: {typeof(ListFieldDesign).FullName}
- TileListFieldDesign: {typeof(TileListFieldDesign).FullName}
";
        }

        private class AIResponse
        {
            public PageFrameDesign PageFrame { get; set; } = new();
            public string Explanation { get; set; } = string.Empty;
        }

        const string SystemPrompt = @"
あなたはローコードWebアプリケーションのページフレーム（ナビゲーション構造）のデザイナです。
ユーザーの指示に基づいてページフレーム定義を編集し、結果をJSONで返してください。

## 基本ルール
- 元のページフレーム定義が渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存のプロパティ値は指示がない限り変更しないでください。
- Name（フレーム識別名）は変更しないでください。
- UserReadConditionは指示がない限り変更しないでください。
- モジュール一覧に存在するモジュール名のみをPageLinkのModuleに指定してください。

## 出力JSON形式

以下の形式でJSONを返してください:
{
  ""PageFrame"": { /* PageFrameDesign - フレーム定義全体 */ },
  ""Explanation"": ""変更内容の説明""
}

- PageFrame: 変更後のページフレーム定義全体
- Explanation: 何を変更したかの簡潔な日本語説明

---

## PageFrameDesign プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| IsApplicationRoot | bool | アプリケーションのルートフレームかどうか |
| Name | string | フレーム識別名（変更不可） |
| Description | string | 説明文 |
| Left | SideBarDesign | 左サイドバー |
| Right | SideBarDesign | 右サイドバー |
| Header | HeaderDesign | ヘッダー |
| Padding | ThicknessDesign | コンテンツ領域の余白 { Left, Top, Right, Bottom } |
| UserReadCondition | ModuleMatchCondition | アクセス権限条件（変更しない） |
| BackgroundColor | string | 背景色 |
| Color | string | 文字色 |
| FontFamily | string | フォントファミリー |
| FontSize | int? | フォントサイズ（**整数のみ。14.0は不可、14と書くこと**） |
| TopPageModuleDesign | ModulePageDesign | デフォルト/ホームページの設定 |
| OtherPageModuleDesigns | List<ModulePageDesign> | サイドバー以外の追加ページ定義 |

## SideBarDesign - サイドバー定義

| プロパティ | 型 | 説明 |
|---|---|---|
| IsVisible | bool | サイドバーを表示するか |
| Home | HomeLabel | ホームリンクの表示設定 |
| Links | List<PageLink> | ナビゲーションリンクのリスト |
| Width | double? | サイドバーの幅（px） |
| Color | string | 文字色 |
| BackgroundColorStart | string | グラデーション開始色 |
| BackgroundColorEnd | string | グラデーション終了色 |
| FontFamily | string | フォントファミリー |
| FontSize | int? | フォントサイズ（**整数のみ**） |
| UserName | string | ユーザー名表示用フィールド |
| ModuleName | string | サイドバー用モジュール名 |

## HeaderDesign - ヘッダー定義

| プロパティ | 型 | 説明 |
|---|---|---|
| IsVisible | bool | ヘッダーを表示するか |
| Home | HomeLabel | ホームリンクの表示設定 |
| Links | List<PageLink> | ヘッダーのナビゲーションリンク |
| Height | double? | ヘッダーの高さ（px） |
| Color | string | 文字色 |
| BackgroundColor | string | 背景色 |
| FontFamily | string | フォントファミリー |
| FontSize | int? | フォントサイズ（**整数のみ**） |
| UserName | string | ユーザー名表示用フィールド |
| ModuleName | string | ヘッダー用モジュール名 |

## HomeLabel - ホームラベル

| プロパティ | 型 | 説明 |
|---|---|---|
| Type | string | ""None"" / ""Text"" / ""Image"" |
| Text | string | テキスト表示時の文字列 |
| Icon | string | アイコンCSSクラス名（例: ""bi bi-house""） |
| ResourcePath | string | 画像表示時のリソースパス |

## PageLink - ナビゲーションリンク（ModulePageDesignを継承）

### PageLink 固有プロパティ
| プロパティ | 型 | 説明 |
|---|---|---|
| Title | string | リンクの表示テキスト |
| Icon | string | アイコン（CSSクラス名またはリソースパス） |
| IconType | string | ""Font"" / ""ResourceImage"" |
| HideTitle | bool | タイトルを非表示にするか（アイコンのみ表示） |

### ModulePageDesign プロパティ（PageLinkが継承）
| プロパティ | 型 | 説明 |
|---|---|---|
| ModulePageType | string | ""Auto"" / ""ListToDetail"" / ""List"" / ""Detail"" |
| ModuleUrlSegment | string | URLのセグメント（カスタムURL用） |
| ActiveModuleSegments | List<string> | アクティブとみなすモジュールセグメントのリスト |
| PageFrame | string | 遷移先のページフレーム名（サブフレーム） |
| Module | string | 遷移先のモジュール名 |
| Id | string | 特定レコードのID（Detail時） |
| Parameters | string | URLパラメータ |
| ListPageDesign | ListPageDesign | 一覧ページの設定 |
| DetailPageDesign | DetailPageDesign | 詳細ページの設定 |

## ListPageDesign - 一覧ページ設定

| プロパティ | 型 | 説明 |
|---|---|---|
| SearchLayoutName | string | 使用するSearchLayout名 |
| UserUrlParameter | bool | 検索条件をURLパラメータに保持するか |
| PageTitle | string | ページタイトル |
| HeaderTitle | string | ヘッダータイトル |
| CanBulkDataUpdate | bool | 一括データ更新を許可 |
| CanBulkDataDownload | bool | 一括データダウンロードを許可 |
| UseSubmitButton | bool | 一覧画面で送信ボタンを使用 |
| UseNavigateToCreate | bool | 新規作成への遷移を許可 |
| ListFieldDesign | ListFieldDesign | 一覧表示のListField設定（TypeFullName必須） |

## DetailPageDesign - 詳細ページ設定

| プロパティ | 型 | 説明 |
|---|---|---|
| PageTitle | string | ページタイトル |
| LayoutName | string | 使用するDetailLayout名 |
| UrlParameter | string | URLパラメータ |

## ModulePageType の選択指針

| 値 | 用途 | 説明 |
|---|---|---|
| Auto | 一般的な場合 | モジュール定義のフィールド構成から自動判定 |
| ListToDetail | 一覧→詳細の遷移型 | 一覧で行選択→詳細画面に遷移 |
| List | 一覧のみ表示 | 詳細画面への遷移なし |
| Detail | 詳細のみ表示 | ダッシュボード、ホーム画面等 |

## PageLink追加時のデフォルト値

新しいPageLinkを追加する場合、以下のデフォルト設定を使用してください:
```json
{
  ""Title"": ""リンク名"",
  ""Icon"": """",
  ""IconType"": ""Font"",
  ""HideTitle"": false,
  ""ModulePageType"": ""Auto"",
  ""ModuleUrlSegment"": """",
  ""ActiveModuleSegments"": [],
  ""PageFrame"": """",
  ""Module"": ""モジュール名"",
  ""Id"": """",
  ""Parameters"": """",
  ""ListPageDesign"": {
    ""SearchLayoutName"": """",
    ""UserUrlParameter"": true,
    ""PageTitle"": """",
    ""HeaderTitle"": """",
    ""CanBulkDataUpdate"": false,
    ""CanBulkDataDownload"": false,
    ""UseSubmitButton"": false,
    ""UseNavigateToCreate"": true,
    ""ListFieldDesign"": {
      ""LayoutName"": """",
      ""CanNavigateToDetail"": true,
      ""NavigateModuleUrlSegment"": """",
      ""CanCustomizeColumns"": false,
      ""DisplayName"": """",
      ""SearchCondition"": {
        ""LimitCount"": 50,
        ""SelectFields"": [],
        ""SortConditions"": [],
        ""SortFieldVariable"": """",
        ""SortDescending"": false,
        ""ModuleName"": """"
      },
      ""PagerPosition"": ""Bottom"",
      ""UseIndexSort"": false,
      ""DeleteTogether"": false,
      ""CanCreate"": false,
      ""CanUpdate"": false,
      ""CanDelete"": true,
      ""CanUserSort"": true,
      ""CanSelect"": false,
      ""Name"": """",
      ""IgnoreModification"": false,
      ""TypeFullName"": ""（TypeFullName一覧のListFieldDesignを参照）""
    }
  },
  ""DetailPageDesign"": {
    ""PageTitle"": """",
    ""LayoutName"": """",
    ""UrlParameter"": """"
  }
}
```

## サブフレームの利用

PageLinkのPageFrameプロパティに別のフレーム名を指定することで、サブフレーム（ネストされたナビゲーション構造）を実現できる。
PageFrame一覧に存在するフレーム名のみ指定可能。

## アイコンについて

Bootstrap Iconsが利用可能。よく使われるアイコンの例:
- bi bi-house - ホーム
- bi bi-people - ユーザー/人物
- bi bi-box - 商品/ボックス
- bi bi-graph-up - グラフ/ダッシュボード
- bi bi-gear - 設定
- bi bi-file-text - ドキュメント
- bi bi-cart - カート/注文
- bi bi-calendar - カレンダー
- bi bi-envelope - メール
- bi bi-search - 検索
- bi bi-list - リスト
- bi bi-pencil - 編集
- bi bi-trash - 削除
- bi bi-plus-circle - 追加
- bi bi-folder - フォルダ
- bi bi-shield-lock - セキュリティ
- bi bi-bar-chart - チャート
- bi bi-building - 会社/組織
- bi bi-clipboard - クリップボード
- bi bi-tags - タグ
";
    }
}
