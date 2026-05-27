using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using OpenAI.Chat;

namespace LowCodeApp.WinForms.Designer.Lib.AI
{
    public class SearchLayoutChat : IAIChat
    {
        readonly IModuleSearchLayoutEditor _editor;
        readonly ChatClient _chatClient;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "検索レイアウトを編集するためのチャットです";

        public SearchLayoutChat(AISettings settings, IModuleSearchLayoutEditor editor)
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
            var search = _editor.GetSearchLayoutDesign();
            if (search.Layout is not SearchGridLayoutDesign)
                return "レイアウトデータが不正です（SearchGridLayoutDesignが必要です）";

            var prompt = BuildPrompt(message, search);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var response = JsonConverterEx.DeserializeObject<AIResponse>(resultText)!;
                ApplyResponse(search, response);

                _conversationHistory.Add(new UserChatMessage(message));
                _conversationHistory.Add(new AssistantChatMessage(resultText));
                TrimConversationHistory();

                return BuildResultMessage(response);
            }
            catch (Exception ex)
            {
                return $"エラーリトライしてください\r\n{ex.Message}";
            }
        }

        List<ChatMessage> BuildPrompt(string message, SearchLayoutDesign search)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(BuildSystemPrompt()));

            var fields = _editor.GetFieldDesigns();
            var fieldInfo = fields.Select(f => $"  {f.Name} ({f.GetType().Name})").ToList();
            prompt.Add(new SystemChatMessage(
                $"現在のモジュールに定義されているフィールド一覧:\n{string.Join("\n", fieldInfo)}"));

            prompt.Add(new SystemChatMessage(
                $"現在のレイアウト:\n{JsonConverterEx.SerializeObject(search.Layout)}"));

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
        }

        void ApplyResponse(SearchLayoutDesign search, AIResponse response)
        {
            if (response.NewLabels?.Count > 0)
            {
                var fields = _editor.GetFieldDesigns();
                var existingNames = new HashSet<string>(fields.Select(f => f.Name));
                var addedLabels = new List<LabelFieldDesign>();
                foreach (var label in response.NewLabels)
                {
                    if (!string.IsNullOrEmpty(label.Name) && !existingNames.Contains(label.Name))
                    {
                        fields.Add(label);
                        addedLabels.Add(label);
                    }
                }
                response.NewLabels = addedLabels;
            }

            search.Layout = response.Layout;
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

        static string BuildResultMessage(AIResponse response)
        {
            var messages = new List<string> { "変更しました" };
            if (response.NewLabels?.Count > 0)
            {
                var names = response.NewLabels.Select(l => l.Name);
                messages.Add($"ラベルを追加しました: {string.Join(", ", names)}");
            }
            return string.Join("\r\n", messages);
        }

        string BuildSystemPrompt()
        {
            return SystemPrompt + $@"

## TypeFullName一覧（JSONに必ず正確に設定すること）
- FieldLayoutDesign: {typeof(FieldLayoutDesign).FullName}
- SearchGridLayoutDesign: {typeof(SearchGridLayoutDesign).FullName}
- GridLayoutDesign: {typeof(GridLayoutDesign).FullName}
- LabelFieldDesign: {typeof(LabelFieldDesign).FullName}
";
        }

        private class AIResponse
        {
            public SearchGridLayoutDesign Layout { get; set; } = new();
            public List<LabelFieldDesign> NewLabels { get; set; } = new();
        }

        const string SystemPrompt = @"
あなたはローコードでのSearch（検索）画面レイアウトのデザイナです。
ユーザーの指示に基づいて検索フォームのレイアウトを編集し、結果をJSONで返してください。

## 基本ルール
- 元のレイアウトが渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存のプロパティ値（Width, Padding, BackgroundColor, BorderStyle, HorizontalAlignment, VerticalAlignment, CanResize, Operator等）は指示がない限り変更しないでください。
- FieldはFieldLayoutDesignの中でFieldNameで指定します。FieldNameは渡されるフィールド一覧に含まれるもの、または新規追加するラベルのNameを使用してください。

## 最重要ルール: Layout内でのフィールド参照方法

Layout内のGridColumn.Layoutに配置できるのは以下の3種類のみです:
- FieldLayoutDesign（フィールド配置）
- SearchGridLayoutDesign（ネスト検索グリッド - And/Or条件のグループ化に使う）
- GridLayoutDesign（ネストグリッド - 条件グループ化が不要な場合）

**ラベルも含め、すべてのフィールドは必ずFieldLayoutDesignのFieldNameで参照してください。**
LabelFieldDesignをLayout内に直接配置してはいけません。LabelFieldDesignはNewLabelsにのみ入れてください。

正しい例（ラベルをFieldLayoutDesignで参照）:
{""FieldName"": ""名前Label"", ""TypeFullName"": ""...FieldLayoutDesign""}

間違った例（LabelFieldDesignをLayout内に直接配置）:
{""Text"": ""名前"", ""TypeFullName"": ""...LabelFieldDesign""}  ← これは絶対にダメ

## 出力JSON形式

以下の形式でJSONを返してください:
{
  ""Layout"": { /* SearchGridLayoutDesign - ルートレイアウト全体。フィールドはすべてFieldLayoutDesignで参照 */ },
  ""NewLabels"": [ /* 新規追加するLabelFieldDesignの定義配列。追加不要なら空配列[] */ ]
}

- Layout: SearchGridLayoutDesign。ラベルを含む全フィールドはFieldLayoutDesign（FieldName指定）で参照する
- NewLabels: ラベルのフィールド定義。ここで定義した名前をLayout内のFieldLayoutDesign.FieldNameで使う

---

## SearchGridLayoutDesignの特徴（Operator: And/Or）

SearchGridLayoutDesignはGridLayoutDesignを継承し、Operatorプロパティを追加したクラスです。
ルートレイアウトおよびネストされたグリッドにSearchGridLayoutDesignを使用することで、
Grid単位で検索条件の結合方式（And/Or）を指定できます。

- Operator: ""And""（デフォルト）→ そのGrid内の全条件をANDで結合
- Operator: ""Or"" → そのGrid内の全条件をORで結合
- ユーザーから指示がない限りOperatorはデフォルトの""And""のままにする

### ネストによる条件グループ化の例
「名前がXXX かつ（ステータスがAまたはBの場合）」を表現する場合:
ルートSearchGridLayoutDesign (Operator: And)
  ├── Row → Column → FieldLayoutDesign (名前フィールド)
  └── Row → Column → SearchGridLayoutDesign (Operator: Or, ネスト)
                        ├── Row → Column → FieldLayoutDesign (ステータスA)
                        └── Row → Column → FieldLayoutDesign (ステータスB)

---

## ラベルの追加

ユーザーがラベルの追加を要求した場合:
1. NewLabelsにLabelFieldDesignの定義を追加する
2. Layout内ではFieldLayoutDesignを使い、そのLabelFieldDesignのNameをFieldNameに指定して配置する

ラベル追加の要求がない場合はNewLabelsは空配列[]にしてください。

### NewLabelsに入れるLabelFieldDesignの形式
{
  ""Text"": ""表示テキスト"",
  ""Icon"": """",
  ""Style"": ""Default"",
  ""RelativeField"": """",
  ""OnClick"": """",
  ""Name"": ""一意なフィールド名"",
  ""IgnoreModification"": false,
  ""TypeFullName"": ""(TypeFullName一覧のLabelFieldDesignを参照)""
}
- Style: Default（通常テキスト） / H1 / H2 / H3 / H4 / H5 / H6
- RelativeField: 指定すると、そのフィールドの値を表示テキストとして使用する。
- Name: Module内で一意なPascalCase名（例: NameLabel, SearchTitle）
- 既にフィールド一覧に存在する名前は使わないこと

### Layout内でのラベル参照方法（FieldLayoutDesignで参照）
ラベルもFieldLayoutDesignのFieldNameで参照する。他のフィールドと同じ方法。
{
  ""FieldName"": ""名前Label"",
  ""Name"": """",
  ""TypeFullName"": ""(TypeFullName一覧のFieldLayoutDesignを参照)""
}

### ラベル配置パターン1: 横並び（ラベルを左、フィールドを右）
1つのRowにラベルColumnとフィールドColumnを横に並べる。
Row
  ├── Column (Width: 150, VerticalAlignment: Middle) → FieldLayoutDesign (FieldName: ラベル名)
  └── Column (Width: null) → FieldLayoutDesign (FieldName: 入力フィールド名)
※入力フィールドのColumnにはHorizontalAlignmentを設定しない（横幅が崩れるため）

### ラベル配置パターン2: 縦並び（ラベルを上、フィールドを下にネストGridで配置）
Column内にネストしたGridLayoutDesignを使い、ラベルと入力フィールドを縦に並べる。
ラベル行のMargin.Bottomを0にしてラベルと入力欄の間隔を詰める。
※ラベル配置用のネストGridはSearchGridLayoutDesignではなくGridLayoutDesignを使ってよい（検索条件のグループ化が目的ではないため）。
**※このパターンではネストGrid内のColumnにWidth, VerticalAlignmentを設定しない。パターン1のWidth: 150やVerticalAlignment: Middleを混ぜないこと。**
外側のRow
  └─ Column
       └─ GridLayoutDesign (ネスト, TypeFullNameを設定)
            ├─ Row (Margin: { ""Bottom"": 0 }) ← ラベル行
            │    └─ Column → FieldLayoutDesign (FieldName: ラベル名)
            └─ Row ← 入力フィールド行
                 └─ Column → FieldLayoutDesign (FieldName: 入力フィールド名)
横に複数項目を並べるときは、1つの外側Rowに複数のColumnを配置し、各Column内にそれぞれネストGridを持たせる。

---

## レイアウトクラス定義

LayoutDesignBaseは抽象クラスで持たれるため、JSONから復元するときに元の型が必要です。TypeFullNameは必ず入れてください。

// SearchGridLayoutDesignはGridLayoutDesignを継承し、Operatorを追加
public class SearchGridLayoutDesign : GridLayoutDesign
{
    public string TypeFullName { get; set; }
    public SearchOperator Operator { get; set; } = SearchOperator.And;// Grid単位のAnd/Or切り替え
}
public enum SearchOperator { UserSpecified, And, Or }

public class FieldLayoutDesign : LayoutDesignBase
{
    public string TypeFullName { get; set; }
    public string FieldName { get; set; } = string.Empty;// 必須。配置するフィールド名。完全一致。
    public string ContextMenu { get; set; } = string.Empty;
    public bool? IsViewOnly { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public int? FontSize { get; set; }
    public CssFontWeight? FontWeight { get; set; }
    public CssFontStyle? FontStyle { get; set; }
    public string Color { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
}
public class GridLayoutDesign : LayoutDesignBase
{
    public string TypeFullName { get; set; }
    public override string Name { get; set; } = string.Empty;
    public bool? IsViewOnly { get; set; }// LayoutDesignBase共通。True：配下すべてを閲覧専用にする。
    public ThicknessDesign Padding { get; set; } = new();
    public bool IsBordered { get; set; }
    public bool UseBorderedShrinkWrap { get; set; }
    public bool IsExpandable { get; set; }
    public string ExpanderLabel { get; set; } = string.Empty;
    public bool IsExpanderDefaultOpened { get; set; }
    public bool IsFlowLayout { get; set; }
    public bool IsAutoFillWrap { get; set; }// 均等折り返し。カラムのMinWidth必須。IsAutoFillWrap時はMaxWidthは無効。
    public bool IsFillAvailable { get; set; }
    public ScrollDirection ScrollDirection { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
    public List<GridRow> Rows { get; set; } = new();
}
public class GridRow
{
    public bool IsWrap { get; set; }
    public bool IsAutoFillWrap { get; set; }// 均等折り返し。カラムのMinWidth必須。IsAutoFillWrap時はMaxWidthは無効。
    public double? Height { get; set; }
    public ThicknessDesign Margin { get; set; } = new();// ユーザーからマージンの指示があった場合に設定する。値の指示がなければ適切な値を自動で設定する。
    public GridRowType GridRowType { get; set; }
    public bool CanResize { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
    public virtual List<GridColumn> Columns { get; init; } = new();
}
public class GridColumn
{
    public LayoutDesignBase? Layout { get; set; }//nullは出力しない
    public double? Width { get; set; }
    public double? MinWidth { get; set; }// 最小幅（px）。IsAutoFillWrapで使用。
    public double? MaxWidth { get; set; }// 最大幅（px）。MinWidthと組み合わせ。※IsAutoFillWrap時は無効。
    public ThicknessDesign Padding { get; set; } = new();// ユーザーからパディングの指示があった場合に設定する。値の指示がなければ適切な値を自動で設定する。
    public string BackgroundColor { get; set; } = string.Empty;

    //ユーザーから囲うまたは枠の指示があったら設定する。数値の指定がない場合は1を設定する。
    //行に対して指示が出た場合は行の中のColumnが対象となる。
    //行に対して指示が出たら、Column毎に枠をすべて同じ設定をするとは限らない
    //例えば行を囲う指示が出たら、列番号が一番低いColumnは上下と左を設定する。右は設定しない。
    public BorderStyleDesign BorderStyle { get; set; } = new();

    public HorizontalAlignment? HorizontalAlignment { get; set; }
    public VerticalAlignment? VerticalAlignment { get; set; }
    public bool CanResize { get; set; }
}
public class ThicknessDesign
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Right { get; set; }
    public double? Bottom { get; set; }
}
public enum GridRowType { Normal, Header, Footer }
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
public enum HorizontalAlignment { Start, Center, End, Stretch }
public enum VerticalAlignment { Top, Middle, Bottom, Stretch }
public enum ScrollDirection { Unset, Vertical, Horizontal }

---

## レイアウトの推奨ルール
- 入力フィールド（TextField, NumberField, DateField等）のColumnにはHorizontalAlignmentを設定しない（入力欄の横幅が崩れるため）
- ラベルのColumnにはHorizontalAlignmentを必要に応じて設定可
- 検索フォームではフィールドを横に並べるのが一般的（1行に複数フィールド）
- 検索フォームのSearchGridLayoutDesignにはIsBordered: true, IsExpandable: true, ExpanderLabel: ""検索条件"", IsExpanderDefaultOpened: falseを推奨（折りたたみ可能にして一覧表示を広くする）

## IsViewOnly の正しい使い方（重要）

IsViewOnly はレイアウト要素（FieldLayoutDesign, GridLayoutDesign, SearchGridLayoutDesign）のプロパティであり、**フィールド定義（Fields配列内）のプロパティではない**。フィールド定義に書いてもデシリアライズ時に無視される。

### IsUpdateProtected との違い
- IsUpdateProtected: フィールド定義のプロパティ。サーバーサイドでもチェックが入り、API経由での更新も防止。
- IsViewOnly: レイアウト要素のプロパティ。表示上の読み取り専用。混同しないこと。

";
    }
}
