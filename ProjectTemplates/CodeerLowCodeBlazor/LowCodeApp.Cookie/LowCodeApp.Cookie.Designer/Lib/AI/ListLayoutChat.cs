using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using OpenAI.Chat;

namespace LowCodeApp.Cookie.Designer.Lib.AI
{
    public class ListLayoutChat : IAIChat
    {
        readonly IModuleListLayoutEditor _editor;
        readonly ChatClient _chatClient;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "一覧レイアウトを編集するためのチャットです";

        public ListLayoutChat(AISettings settings, IModuleListLayoutEditor editor)
        {
            _editor = editor;
            var azureClient = new AzureOpenAIClient(
                new Uri(settings.OpenAIEndPoint),
                new ApiKeyCredential(settings.OpenAIKey));
            _chatClient = azureClient.GetChatClient(settings.ChatModel);
        }

        public void Clear() => _conversationHistory.Clear();

        public async Task<string> ProcessMessage(string message)
        {
            var list = _editor.GetListLayoutDesign();

            var prompt = BuildPrompt(message, list);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var response = JsonConverterEx.DeserializeObject<AIResponse>(resultText)!;
                list.Elements = response.Elements;
                _editor.Update();

                _conversationHistory.Add(new UserChatMessage(message));
                _conversationHistory.Add(new AssistantChatMessage(resultText));
                TrimConversationHistory();

                return "変更しました";
            }
            catch (Exception ex)
            {
                return $"エラーリトライしてください\r\n{ex.Message}";
            }
        }

        List<ChatMessage> BuildPrompt(string message, ListLayoutDesign list)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(SystemPrompt));

            var fields = _editor.GetFieldDesigns();
            var fieldInfo = fields.Select(f => $"  {f.Name} ({f.GetType().Name})").ToList();
            prompt.Add(new SystemChatMessage(
                $"現在のモジュールに定義されているフィールド一覧:\n{string.Join("\n", fieldInfo)}"));

            prompt.Add(new SystemChatMessage(
                $"現在のレイアウト:\n{JsonConverterEx.SerializeObject(list.Elements)}"));

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
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
            public List<List<ListElement>> Elements { get; set; } = [[]];
        }

        const string SystemPrompt = @"
あなたはローコードでのList（一覧）画面レイアウトのデザイナです。
ユーザーの指示に基づいて一覧テーブルの列定義を編集し、結果をJSONで返してください。

## 基本ルール
- 元のレイアウトが渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存の列のプロパティ値（Width, CanResize, CanUserSort, HorizontalAlignment等）は指示がない限り変更しないでください。
- FieldNameは渡されるフィールド一覧に含まれるものを使用してください。

## 行追加と結合は別の操作（重要）
- 「段を増やす」「行を追加する」＝ 既存行と同じ列数の空行を追加する。既存セルは一切変更しない（RowSpan変更もフィールド移動もしない）。
  - 空行の各セルは { ""FieldName"": """", ""ColumnSpan"": 1, ""RowSpan"": 1 } とする。空の配列 [] は不可。
- 「結合する」「またがる」＝ RowSpan / ColumnSpan を使う。ユーザーが明示的に結合を指示した場合のみ。
- ユーザーが結合を指示していないのに RowSpan を 2以上にしたり、結合用プレースホルダーを作ったりしないこと。

## 出力JSON形式

以下の形式でJSONを返してください:
{
  ""Elements"": [ /* List<List<ListElement>> - テーブル列定義 */ ]
}

---

## Elementsの構造

Elementsは「ヘッダー行の配列」であり、各行は「列（ListElement）の配列」です。

### 通常パターン（1行ヘッダー）
ほとんどの場合、ヘッダーは1行です。全列を1つの内側配列にまとめます。
{
  ""Elements"": [
    [
      { ""FieldName"": ""Id"", ""Label"": ""ID"", ""Width"": 100, ""ColumnSpan"": 1, ""RowSpan"": 1, ""CanResize"": true, ""CanUserSort"": true },
      { ""FieldName"": ""Name"", ""Label"": ""名前"", ""ColumnSpan"": 1, ""RowSpan"": 1, ""CanResize"": true, ""CanUserSort"": true },
      { ""FieldName"": ""Price"", ""Label"": ""価格"", ""Width"": 120, ""ColumnSpan"": 1, ""RowSpan"": 1, ""CanResize"": true, ""CanUserSort"": true, ""HorizontalAlignment"": ""End"" }
    ]
  ]
}

重要: [[Col1, Col2, Col3]] が正しい形式です。
[[Col1], [Col2], [Col3]] は3行ヘッダーになってしまうので間違いです。

### 複数行ヘッダー
ヘッダーを複数行にする場合のみ外側の配列に行を追加し、RowSpan / ColumnSpan でセル結合を制御します。

**FieldName のルール（重要）:**
- データを表示するセルは必ず FieldName にフィールド名を設定する（FieldName: """" は不可）
- FieldName: """" は**RowSpanで上のセルに結合されている位置のプレースホルダーとしてのみ**使用する
- FieldName: """" のセルには Label や他のプロパティを設定しない（空のプレースホルダーのみ）

**行の列数ルール:**
- 各行の論理的な列数（ColumnSpanを考慮）は、RowSpanによる結合分を除いて一致させる
- 例: 行0で3列、Idが RowSpan:2 の場合 → 行1はIdのプレースホルダー1列 + データ2列 = 3列

例: Idが2行にまたがり、他の列は行ごとに異なるフィールドを表示
{
  ""Elements"": [
    [
      { ""FieldName"": ""Id"", ""Label"": ""ID"", ""RowSpan"": 2 },
      { ""FieldName"": ""Name"", ""Label"": ""名前"" },
      { ""FieldName"": ""Price"", ""Label"": ""価格"" }
    ],
    [
      { ""FieldName"": """" },
      { ""FieldName"": ""Category"", ""Label"": ""カテゴリ"" },
      { ""FieldName"": ""Stock"", ""Label"": ""在庫"" }
    ]
  ]
}
行1の先頭 { ""FieldName"": """" } はIdの RowSpan:2 に覆われた位置のプレースホルダー。
行1の Category と Stock はデータ表示セルなので FieldName にフィールド名が必須。

---

## ListElementのプロパティ

public class ListElement
{
    public string FieldName { get; set; } = string.Empty;// 必須。表示するフィールド名。完全一致。
    public string ContextMenu { get; set; } = string.Empty;// 右クリックメニュー定義の参照名。
    public string Label { get; set; } = string.Empty;// 列ヘッダーのラベルテキスト。
    public double? Width { get; set; }// 列幅（px）。nullで自動。
    public int ColumnSpan { get; set; } = 1;// 列結合数。整数のみ（1.0は不可）。
    public int RowSpan { get; set; } = 1;// 行結合数。整数のみ（1.0は不可）。
    public bool? IsViewOnly { get; set; }// True：読み取り専用で表示。
    public TextWrap TextWrap { get; set; }// テキスト折り返し。
    public bool CanResize { get; set; }// ユーザーによる列幅変更を許可。
    public bool CanUserSort { get; set; } = true;// 列ヘッダークリックでのソートを許可。
    public HorizontalAlignment? HeaderHorizontalAlignment { get; set; }// ヘッダーの水平配置。
    public HorizontalAlignment? HorizontalAlignment { get; set; }// データセルの水平配置。
    public string ClassName { get; set; } = string.Empty;// CSSクラス名。
    public string FontFamily { get; set; } = string.Empty;
    public int? FontSize { get; set; }// 整数のみ（14.0は不可、14と書くこと）。
    public CssFontWeight? FontWeight { get; set; }
    public CssFontStyle? FontStyle { get; set; }
    public string Color { get; set; } = string.Empty;// 文字色。
    public string BackgroundColor { get; set; } = string.Empty;// 背景色。
    public string DetailLayoutName { get; set; } = string.Empty;// インライン編集時に使用するDetailLayout名。
    public string ListElementComponent { get; set; } = string.Empty;// カスタムコンポーネント名。
}

public enum TextWrap { Unset, BreakAll, Ellipsis }
public enum HorizontalAlignment { Start, Center, End, Stretch }

---

## レイアウトの推奨ルール
- 数値系フィールド（NumberField等）のHorizontalAlignmentは""End""（右寄せ）が見やすい
- ID列やコード列は固定幅（Width指定）にするとよい
- メインのテキスト列（名前等）はWidth: nullで自動幅にする
- ColumnSpan, RowSpan, FontSizeは整数のみ（1.0ではなく1と書く）
- CanResizeは通常trueにしてユーザーが列幅を調整できるようにする

## IsViewOnly の正しい使い方（重要）

IsViewOnly はListElementのプロパティであり、**フィールド定義（Fields配列内）のプロパティではない**。フィールド定義に書いてもデシリアライズ時に無視される。

### 正しい例
ListElement に設定: {""FieldName"": ""Total"", ""Label"": ""合計"", ""IsViewOnly"": true}

### IsUpdateProtected との違い
- IsUpdateProtected: フィールド定義のプロパティ。サーバーサイドでもチェックが入り、API経由での更新も防止。
- IsViewOnly: レイアウト要素（ListElement等）のプロパティ。表示上の読み取り専用。混同しないこと。

";
    }
}
