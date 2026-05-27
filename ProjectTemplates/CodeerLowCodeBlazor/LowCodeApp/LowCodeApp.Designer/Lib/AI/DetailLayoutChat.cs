using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using OpenAI.Chat;

namespace LowCodeApp.Designer.Lib.AI
{
    public class DetailLayoutChat : IAIChat
    {
        readonly IModuleDetailLayoutEditor _editor;
        readonly ChatClient _chatClient;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "詳細レイアウトを編集するためのチャットです";

        public DetailLayoutChat(AISettings settings, IModuleDetailLayoutEditor editor)
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
            var detail = _editor.GetDetailLayoutDesign();
            if (detail.Layout is not GridLayoutDesign)
                return "レイアウトデータが不正です（GridLayoutDesignが必要です）";

            var prompt = BuildPrompt(message, detail);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var response = JsonConverterEx.DeserializeObject<AIResponse>(resultText)!;
                ApplyResponse(detail, response);

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

        List<ChatMessage> BuildPrompt(string message, DetailLayoutDesign detail)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(BuildSystemPrompt()));

            var fields = _editor.GetFieldDesigns();
            var fieldInfo = fields.Select(f => $"  {f.Name} ({f.GetType().Name})").ToList();
            prompt.Add(new SystemChatMessage(
                $"現在のモジュールに定義されているフィールド一覧:\n{string.Join("\n", fieldInfo)}"));

            prompt.Add(new SystemChatMessage(
                $"現在のレイアウト:\n{JsonConverterEx.SerializeObject(detail.Layout)}"));

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
        }

        void ApplyResponse(DetailLayoutDesign detail, AIResponse response)
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

            detail.Layout = response.Layout;
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
- GridLayoutDesign: {typeof(GridLayoutDesign).FullName}
- CanvasLayoutDesign: {typeof(CanvasLayoutDesign).FullName}
- TabLayoutDesign: {typeof(TabLayoutDesign).FullName}
- LabelFieldDesign: {typeof(LabelFieldDesign).FullName}
";
        }

        private class AIResponse
        {
            public GridLayoutDesign Layout { get; set; } = new();
            public List<LabelFieldDesign> NewLabels { get; set; } = new();
        }

        const string SystemPrompt = @"
あなたはローコードでのDetail画面レイアウトのデザイナです。
ユーザーの指示に基づいてレイアウトを編集し、結果をJSONで返してください。

## 基本ルール
- 元のレイアウトが渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存のプロパティ値（Width, Padding, BackgroundColor, BorderStyle, HorizontalAlignment, VerticalAlignment, CanResize等）は指示がない限り変更しないでください。
- FieldはFieldLayoutDesignの中でFieldNameで指定します。FieldNameは渡されるフィールド一覧に含まれるもの、または新規追加するラベルのNameを使用してください。

## フィールド配置は移動（重複禁止）
- レイアウト内で同じFieldNameのFieldLayoutDesignは**1箇所のみ**に存在できます。
- ユーザーが既にレイアウトに配置されているフィールドを別の場所に「配置して」「追加して」「入れて」と指示した場合、それは**移動**を意味します。
  - 新しい位置にFieldLayoutDesignを配置する
  - **元の位置からは必ず削除する**（ColumnのLayoutをnullにする＝Layoutプロパティを出力しない）
- 同じフィールドが複数箇所に出現するJSONは不正です。絶対に出力しないでください。

## 最重要ルール: Layout内でのフィールド参照方法

Layout内のGridColumn.Layoutに配置できるのは以下の4種類のみです:
- FieldLayoutDesign（フィールド配置）
- GridLayoutDesign（ネストグリッド）
- TabLayoutDesign（タブ）
- CanvasLayoutDesign（キャンバス）

**ラベルも含め、すべてのフィールドは必ずFieldLayoutDesignのFieldNameで参照してください。**
LabelFieldDesignをLayout内に直接配置してはいけません。LabelFieldDesignはNewLabelsにのみ入れてください。

正しい例（ラベルをFieldLayoutDesignで参照）:
{""FieldName"": ""名前Label"", ""TypeFullName"": ""...FieldLayoutDesign""}

間違った例（LabelFieldDesignをLayout内に直接配置）:
{""Text"": ""名前"", ""TypeFullName"": ""...LabelFieldDesign""}  ← これは絶対にダメ

## 出力JSON形式

以下の形式でJSONを返してください:
{
  ""Layout"": { /* GridLayoutDesign - ルートレイアウト全体。フィールドはすべてFieldLayoutDesignで参照 */ },
  ""NewLabels"": [ /* 新規追加するLabelFieldDesignの定義配列。追加不要なら空配列[] */ ]
}

- Layout: レイアウト構造。ラベルを含む全フィールドはFieldLayoutDesign（FieldName指定）で参照する
- NewLabels: ラベルのフィールド定義。ここで定義した名前をLayout内のFieldLayoutDesign.FieldNameで使う

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
- RelativeField: 指定すると、そのフィールドの値を表示テキストとして使用する。HTMLのlabel要素のfor属性も設定されアクセシビリティ対応する。入力フィールドのラベルにはこれを設定するのが望ましい。
- Name: Module内で一意なPascalCase名（例: NameLabel, PriceLabel, SectionTitle）
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

public class FieldLayoutDesign : LayoutDesignBase
{
    public string TypeFullName { get; set; }
    public string FieldName { get; set; } = string.Empty;// 必須。配置するフィールド名。完全一致。
    public string ContextMenu { get; set; } = string.Empty;// 右クリックメニュー定義の参照名。空はメニューなし。
    public bool? IsViewOnly { get; set; }// True：閲覧・表示のみ。False：編集可能。
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
public class CanvasLayoutDesign : LayoutDesignBase
{
    public string TypeFullName { get; set; }
    public List<CanvasElement> Elements { get; set; } = new();
    public bool IsBordered { get; set; }
    public ScrollDirection ScrollDirection { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
}

// ユーザーからタブレイアウトの指示があった場合に設定する。タブレイアウトの指示がない場合は出力しない。
public class TabLayoutDesign : LayoutDesignBase
{
    public string TypeFullName { get; set; }
    public override string Name { get; set; } = string.Empty;
    public List<string> Tabs{ get; set; } = new();
    public ThicknessDesign Padding { get; set; } = new();// ユーザーからパディングの指示があった場合に設定する。値の指示がなければ適切な値を自動で設定する。
    public bool IsBordered { get; set; }
    public string Color { get; set; } = string.Empty;
    public string SelectedColor { get; set; } = string.Empty;
    public virtual List<LayoutDesignBase> Layouts { get; set; } = new();
    public string OnSelectedIndexChanged { get; set; } = string.Empty;
    public string OnSelectedIndexChanging { get; set; } = string.Empty;
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
- ボタンが行に1つだけの場合はHorizontalAlignment: Centerを設定

## IsViewOnly の正しい使い方（重要）

IsViewOnly はレイアウト要素（FieldLayoutDesign, GridLayoutDesign）のプロパティであり、**フィールド定義（Fields配列内）のプロパティではない**。フィールド定義に書いてもデシリアライズ時に無視される。

### 設定場所の階層と優先順位
子要素の設定が親要素の設定を上書きする。
- GridLayoutDesign (IsViewOnly) ← グリッド全体を閲覧専用にする
  - FieldLayoutDesign (IsViewOnly) ← 個別フィールドの閲覧専用を制御

### 正しい例
- FieldLayoutDesign に設定: {""FieldName"": ""Total"", ""IsViewOnly"": true, ""TypeFullName"": ""...FieldLayoutDesign""}
- GridLayoutDesign に設定（配下全フィールドが閲覧専用）: {""Rows"": [...], ""IsViewOnly"": true, ""TypeFullName"": ""...GridLayoutDesign""}

### IsUpdateProtected との違い
- IsUpdateProtected: フィールド定義のプロパティ。サーバーサイドでもチェックが入り、API経由での更新も防止。
- IsViewOnly: レイアウト要素のプロパティ。表示上の読み取り専用。混同しないこと。

## TabLayoutDesignの配置ルール（空のColumnに追加）
- ユーザーから配置場所の指定がない場合は、必ず既存の「空のColumn」に追加する。
- 「空のColumn」とは、GridColumnにLayoutプロパティが存在しない（出力されていない）Columnを指す。
  - Layout:nullは出力しないため、nullを入れず「Layout自体が無いColumn」を空とみなす。
- 空のColumnが複数ある場合：
  - ユーザーが行/列を指定していればそのColumnを優先する。
  - 指定が無ければ、上から・左から最初に見つかった空のColumnを使う。
- 既存のColumnのWidth / Padding / BackgroundColor / BorderStyle / HorizontalAlignment / VerticalAlignment / CanResize などは変更しない。
  - 変更するのは、そのColumnにLayout（TabLayoutDesign）を追加することのみ。
- 空のColumnが1つも無い場合のみ、最小限の追加でColumn（またはRow）を増やして配置する。

## TabLayoutDesignの整合性ルール（追加タブの初期レイアウト形を固定）
- TabLayoutDesignを出力する場合、TabsとLayoutsの要素数は必ず一致させる（Tabs.Count == Layouts.Count）。
- Tabsが1つ以上あるのにLayoutsが空（[]）のままは不可。
- Tabsを追加した場合は、同じインデックス位置に対応するLayoutsも必ず1つ追加する（既存Layoutsは変更しない）。

#### 追加したタブの初期Layout（必須）
- ユーザーから追加タブ内のレイアウト指示が無い場合、追加したタブ用Layouts要素は必ず次の「初期GridLayoutDesign」を入れる：
  - GridLayoutDesign.Rowsを1行だけ持つ（Rows.Count = 1）
  - その1行のColumnsは4列にする（Columns.Count = 4）
  - 4列すべてのGridColumnはLayoutを持たない空のColumnにする（Layoutプロパティを出力しない）
  - Row/Columnの他プロパティは既定値でよい

";
    }
}
