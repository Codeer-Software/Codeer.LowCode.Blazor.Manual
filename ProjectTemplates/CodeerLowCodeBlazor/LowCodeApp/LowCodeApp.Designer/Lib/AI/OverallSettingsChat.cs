using System.ClientModel;
using Azure.AI.OpenAI;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using OpenAI.Chat;

namespace LowCodeApp.Designer.Lib.AI
{
    public class OverallSettingsChat : IAIChat
    {
        readonly IModuleOverallSettingsEditor _editor;
        readonly ChatClient _chatClient;
        readonly DesignerEnvironment _designerEnvironment;
        readonly List<ChatMessage> _conversationHistory = new();

        public string Explanation => "モジュールとフィールドの設定を編集するためのチャットです";

        public OverallSettingsChat(DesignerEnvironment designerEnvironment, AISettings settings, IModuleOverallSettingsEditor editor)
        {
            _designerEnvironment = designerEnvironment;
            _editor = editor;
            var azureClient = new AzureOpenAIClient(
                new Uri(settings.OpenAIEndPoint),
                new ApiKeyCredential(settings.OpenAIKey));
            _chatClient = azureClient.GetChatClient(settings.ChatModel);
        }

        public void Clear() => _conversationHistory.Clear();

        public class ModuleDesignEditing
        {
            public string DataSourceName { get; set; } = string.Empty;
            public string DbTable { get; set; } = string.Empty;

            public bool CanCreate { get; set; } = true;
            public bool CanUpdate { get; set; } = true;
            public bool CanDelete { get; set; } = true;

            public ModuleMatchCondition UserWriteCondition { get; set; } = new();
            public ModuleMatchCondition UserReadCondition { get; set; } = new();
            public ModuleMatchCondition DataWriteCondition { get; set; } = new();
            public ModuleMatchCondition DataReadCondition { get; set; } = new();

            public List<FieldDesignBase> Fields { get; set; } = new();
        }

        public class IO
        {
            public string ModuleName { get; set; } = string.Empty;
            public ModuleDesignEditing ModuleDesign { get; set; } = new();
            public List<string> NeedModuleInfo { get; set; } = new();
            public string Explanation { get; set; } = string.Empty;
        }

        public class ModuleInfo
        {
            public string Name { get; set; } = string.Empty;
            public Dictionary<string, string> FieldNameAndTypes { get; set; } = new();
        }

        public async Task<string> ProcessMessage(string message)
            => await ProcessMessageCore(message, new());

        async Task<string> ProcessMessageCore(string message, List<string> needModuleInfo)
        {
            var designData = _designerEnvironment.GetDesignData();
            var currentMods = new List<ModuleInfo>();
            foreach (var modName in needModuleInfo)
            {
                var mod = designData.Modules.Find(modName);
                if (mod == null) return $"モジュール{modName}が見つかりませんでした。";
                var fieldNameAndTypes = new Dictionary<string, string>();
                foreach (var field in mod.Fields)
                {
                    fieldNameAndTypes[field.Name] = field.GetType().Name;
                }
                currentMods.Add(new ModuleInfo { Name = mod.Name, FieldNameAndTypes = fieldNameAndTypes });
            }

            var prompt = BuildPrompt(message, designData, currentMods, needModuleInfo);

            var result = await _chatClient.CompleteChatAsync(prompt,
                new ChatCompletionOptions
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                });
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                var output = JsonConverterEx.DeserializeObject<IO>(resultText)!;
                if (output.NeedModuleInfo.Any())
                {
                    var x = needModuleInfo.ToList();
                    x.AddRange(output.NeedModuleInfo);
                    return await ProcessMessageCore(message, x.Distinct().ToList());
                }

                var editingModule = _editor.GetModuleDesign();
                editingModule.DataSourceName = output.ModuleDesign.DataSourceName;
                editingModule.DbTable = output.ModuleDesign.DbTable;
                editingModule.CanCreate = output.ModuleDesign.CanCreate;
                editingModule.CanUpdate = output.ModuleDesign.CanUpdate;
                editingModule.CanDelete = output.ModuleDesign.CanDelete;
                editingModule.UserWriteCondition = output.ModuleDesign.UserWriteCondition;
                editingModule.UserReadCondition = output.ModuleDesign.UserReadCondition;
                editingModule.DataWriteCondition = output.ModuleDesign.DataWriteCondition;
                editingModule.DataReadCondition = output.ModuleDesign.DataReadCondition;
                editingModule.Fields = output.ModuleDesign.Fields;
                _editor.Update();

                _conversationHistory.Add(new UserChatMessage(message));
                _conversationHistory.Add(new AssistantChatMessage(resultText));
                TrimConversationHistory();

                return string.IsNullOrEmpty(output.Explanation)
                    ? "変更しました"
                    : output.Explanation;
            }
            catch (Exception ex)
            {
                return $"エラーリトライしてください\r\n{ex.Message}";
            }
        }

        List<ChatMessage> BuildPrompt(string message, DesignData designData, List<ModuleInfo> currentMods, List<string> needModuleInfo)
        {
            var prompt = new List<ChatMessage>();

            prompt.Add(new SystemChatMessage(BuildSystemPrompt()));
            prompt.Add(new SystemChatMessage(BuildDesignContextInfo(designData)));

            var editingModule = _editor.GetModuleDesign();
            var input = new IO
            {
                ModuleName = editingModule.Name,
                ModuleDesign = new ModuleDesignEditing
                {
                    DataSourceName = editingModule.DataSourceName,
                    DbTable = editingModule.DbTable,
                    CanCreate = editingModule.CanCreate,
                    CanUpdate = editingModule.CanUpdate,
                    CanDelete = editingModule.CanDelete,
                    UserWriteCondition = editingModule.UserWriteCondition,
                    UserReadCondition = editingModule.UserReadCondition,
                    DataWriteCondition = editingModule.DataWriteCondition,
                    DataReadCondition = editingModule.DataReadCondition,
                    Fields = editingModule.Fields,
                },
                NeedModuleInfo = needModuleInfo
            };

            prompt.Add(new SystemChatMessage(
                $"現在のモジュールの設定です。\n{JsonConverterEx.SerializeObject(input)}"));

            if (currentMods.Any())
            {
                prompt.Add(new SystemChatMessage(
                    $"その他のモジュールの情報です。\n{JsonConverterEx.SerializeObject(currentMods)}"));
            }

            prompt.AddRange(_conversationHistory);
            prompt.Add(new UserChatMessage(message));

            return prompt;
        }

        string BuildDesignContextInfo(DesignData designData)
        {
            var lines = new List<string> { "## 現在のアプリケーション情報" };

            try
            {
                var moduleNames = designData.Modules.GetModuleNames();
                if (moduleNames.Any())
                {
                    lines.Add("\n### モジュール一覧（LinkField等の参照先として使用可能）");
                    foreach (var name in moduleNames)
                    {
                        var mod = designData.Modules.Find(name);
                        if (mod == null) continue;
                        var fieldSummary = mod.Fields.Select(f => $"{f.Name}({f.GetType().Name})");
                        lines.Add($"- {name}: {string.Join(", ", fieldSummary)}");
                    }
                }

                var settings = _designerEnvironment.GetDesignerSettings();
                if (settings.DataSources.Any())
                {
                    lines.Add("\n### データソース一覧（DataSourceNameに指定可能）");
                    foreach (var ds in settings.DataSources)
                    {
                        lines.Add($"- {ds.Name} ({ds.DataSourceType})");
                    }
                }
            }
            catch
            {
                lines.Add("（デザインデータの取得に失敗しました）");
            }

            return string.Join("\n", lines);
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

## TypeFullName一覧（JSONに必ず正確に設定すること）

### MatchCondition系
- FieldValueMatchCondition: {typeof(FieldValueMatchCondition).FullName}
- FieldValueMatchConditionNonNull: {typeof(FieldValueMatchConditionNonNull).FullName}
- FieldVariableMatchCondition: {typeof(FieldVariableMatchCondition).FullName}
- MultiMatchCondition: {typeof(MultiMatchCondition).FullName}

### 入力系フィールド
- IdFieldDesign: {typeof(IdFieldDesign).FullName}
- TextFieldDesign: {typeof(TextFieldDesign).FullName}
- NumberFieldDesign: {typeof(NumberFieldDesign).FullName}
- BooleanFieldDesign: {typeof(BooleanFieldDesign).FullName}
- DateFieldDesign: {typeof(DateFieldDesign).FullName}
- DateTimeFieldDesign: {typeof(DateTimeFieldDesign).FullName}
- TimeFieldDesign: {typeof(TimeFieldDesign).FullName}
- PasswordFieldDesign: {typeof(PasswordFieldDesign).FullName}
- SelectFieldDesign: {typeof(SelectFieldDesign).FullName}
- RadioGroupFieldDesign: {typeof(RadioGroupFieldDesign).FullName}
- RadioButtonFieldDesign: {typeof(RadioButtonFieldDesign).FullName}
- LinkFieldDesign: {typeof(LinkFieldDesign).FullName}
- FileFieldDesign: {typeof(FileFieldDesign).FullName}
- JsonFieldDesign: {typeof(JsonFieldDesign).FullName}

### 表示系フィールド
- LabelFieldDesign: {typeof(LabelFieldDesign).FullName}
- ImageViewerFieldDesign: {typeof(ImageViewerFieldDesign).FullName}
- MarkupStringFieldDesign: {typeof(MarkupStringFieldDesign).FullName}
- AnchorTagFieldDesign: {typeof(AnchorTagFieldDesign).FullName}

### ボタン系フィールド
- ButtonFieldDesign: {typeof(ButtonFieldDesign).FullName}
- SubmitButtonFieldDesign: {typeof(SubmitButtonFieldDesign).FullName}
- CopyModuleButtonFieldDesign: {typeof(CopyModuleButtonFieldDesign).FullName}
- ViewEditToggleButtonFieldDesign: {typeof(ViewEditToggleButtonFieldDesign).FullName}

### 一覧・構造系フィールド
- ListFieldDesign: {typeof(ListFieldDesign).FullName}
- DetailListFieldDesign: {typeof(DetailListFieldDesign).FullName}
- TileListFieldDesign: {typeof(TileListFieldDesign).FullName}
- ListPagingFieldDesign: {typeof(ListPagingFieldDesign).FullName}
- ListNumberFieldDesign: {typeof(ListNumberFieldDesign).FullName}
- SearchFieldDesign: {typeof(SearchFieldDesign).FullName}
- ModuleFieldDesign: {typeof(ModuleFieldDesign).FullName}

### メニュー系フィールド
- HeaderMenuFieldDesign: {typeof(HeaderMenuFieldDesign).FullName}
- SidebarMenuFieldDesign: {typeof(SidebarMenuFieldDesign).FullName}
- ContextMenuFieldDesign: {typeof(ContextMenuFieldDesign).FullName}

### チャート系フィールド（外部ライブラリ）
- ApexChartFieldDesign: {typeof(ApexChartFieldDesign).FullName}
- ApexRadialChartFieldDesign: {typeof(ApexRadialChartFieldDesign).FullName}

### その他
- OptimisticLockingFieldDesign: {typeof(OptimisticLockingFieldDesign).FullName}
- ProCodeFieldDesign: {typeof(ProCodeFieldDesign).FullName}

### MultiTypeValue系（FieldValueMatchConditionのValueに使用）
- StringValue: {typeof(StringValue).FullName}
- DecimalValue: {typeof(DecimalValue).FullName}
- BooleanValue: {typeof(BooleanValue).FullName}
- DateOnlyValue: {typeof(DateOnlyValue).FullName}
- TimeOnlyValue: {typeof(TimeOnlyValue).FullName}
- DateTimeValue: {typeof(DateTimeValue).FullName}
- NullValue: {typeof(NullValue).FullName}
";
        }

        const string SystemPrompt = @"
あなたはローコードWebアプリケーションのモジュール設定（フィールド定義・CRUD権限等）のデザイナです。
ユーザーの指示に基づいてモジュールのフィールドや設定を編集し、結果をJSONで返してください。

## 基本ルール
- 元のModuleDesignが渡されるので、ユーザーの指示に対して**必要最小限の変更**にしてください。
- 既存のフィールドや設定は指示がない限り変更・削除しないでください。
- ModuleNameは変更しないでください。
- フィールド名はModule内で一意である必要があります。PascalCaseを使用してください。
- DBカラム名はsnake_caseを使用してください。
- **JSON数値型に注意**: int型プロパティに14.0のような小数点付き数値を書くとエラーになります。整数は必ず整数で書いてください。

## 出力JSON形式

以下の形式でJSONを返してください:
{
  ""ModuleName"": ""モジュール名（変更しない）"",
  ""ModuleDesign"": { /* ModuleDesignEditing - 設定全体 */ },
  ""NeedModuleInfo"": [],
  ""Explanation"": ""変更内容の説明""
}

- ModuleName: 現在のモジュール名（そのまま返す）
- ModuleDesign: 変更後のモジュール設定全体
- NeedModuleInfo: 他のモジュールの情報が必要な場合にモジュール名のリストを返す（情報を入れて再リクエストします）。不要なら空配列[]
- Explanation: 何を変更したかの簡潔な日本語説明

---

## ModuleDesignEditing プロパティ

| プロパティ | 型 | 説明 |
|---|---|---|
| DataSourceName | string | データソース名 |
| DbTable | string | DBテーブル名（snake_case） |
| CanCreate | bool | 新規作成を許可 |
| CanUpdate | bool | 更新を許可 |
| CanDelete | bool | 削除を許可 |
| UserWriteCondition | ModuleMatchCondition | 書き込み権限条件 |
| UserReadCondition | ModuleMatchCondition | 読み取り権限条件 |
| DataWriteCondition | ModuleMatchCondition | データ書き込み条件 |
| DataReadCondition | ModuleMatchCondition | データ読み取り条件 |
| Fields | List<FieldDesignBase> | フィールド定義のリスト |

---

## システムフィールド

以下の名前のフィールドはシステム予約名で、型も決まっています。指示がない限り変更しないでください。

| Name | 型 | 説明 |
|---|---|---|
| Id | IdFieldDesign | 主キー |
| LogicalDelete | BooleanFieldDesign | 論理削除フラグ |
| CreatedAt | DateTimeFieldDesign | 作成日時 |
| UpdatedAt | DateTimeFieldDesign | 更新日時 |
| Creator | LinkFieldDesign | 作成者 |
| Updater | LinkFieldDesign | 更新者 |
| OptimisticLocking | OptimisticLockingFieldDesign | 楽観ロック |

---

## フィールド型の概要と用途

### 入力系フィールド（DBカラムと紐づく）
- **IdFieldDesign** - 主キー。DbColumn必須。
- **TextFieldDesign** - テキスト入力。IsMultiline, MaxLength, Placeholder等
- **NumberFieldDesign** - 数値入力。Format, Min, Max, Step等
- **BooleanFieldDesign** - チェックボックス/トグル。UIType: CheckBox/ToggleButton/Switch
- **DateFieldDesign** - 日付入力。Format, IsYearMonthOnly
- **DateTimeFieldDesign** - 日時入力。SaveAsUtc, Format
- **TimeFieldDesign** - 時刻入力。SaveAsUtc
- **PasswordFieldDesign** - パスワード入力（DBカラムなし）
- **SelectFieldDesign** - ドロップダウン選択。Candidates（""表示,値""形式）またはSearchCondition+ValueVariable+DisplayTextVariable
- **RadioGroupFieldDesign** + **RadioButtonFieldDesign** - ラジオボタン。RadioButtonのGroupFieldにRadioGroupのNameを指定
- **LinkFieldDesign** - 外部キー。SearchCondition.ModuleNameで参照先モジュール、ValueVariable/DisplayTextVariableで表示
- **FileFieldDesign** - ファイルアップロード。StorageName, DbColumnFileName/FileSize/FileGuid
- **JsonFieldDesign** - JSON構造化データ。DbColumn必須

### 表示系フィールド（DBカラムなし）
- **LabelFieldDesign** - テキストラベル。Text, Style(Default/H1~H6), RelativeField
- **ImageViewerFieldDesign** - 画像表示。ResourcePath, ObjectFit
- **MarkupStringFieldDesign** - HTMLマークアップ表示。ResourcePath or RawHtml
- **AnchorTagFieldDesign** - リンク/ナビゲーション。Target: Url/HistoryBack/HistoryForward。モジュール遷移はTarget=""Url"" + Moduleプロパティで指定。IdVariable

### ボタン系フィールド（DBカラムなし）
- **ButtonFieldDesign** - 汎用ボタン。OnClickでスクリプトイベント指定。Variant: Primary/Secondary/Success/Danger等
- **SubmitButtonFieldDesign** - データ保存ボタン。IsBlock
- **CopyModuleButtonFieldDesign** - レコードコピーボタン
- **ViewEditToggleButtonFieldDesign** - 表示/編集切替ボタン

### 一覧・構造系フィールド
- **ListFieldDesign** - テーブル形式一覧。SearchCondition.ModuleNameで対象モジュール指定
- **DetailListFieldDesign** - 明細リスト（インライン編集）。DeleteTogetherで親と一緒に削除
- **TileListFieldDesign** - タイル/カード形式一覧。TileWidth
- **ListPagingFieldDesign** - ページング。ListFieldNameで対象リスト指定
- **ListNumberFieldDesign** - 行番号表示
- **SearchFieldDesign** - 検索フォーム。ResultsViewFieldNameで結果表示先指定
- **ModuleFieldDesign** - 他モジュール埋め込み。ModuleNameで対象モジュール指定

### メニュー系フィールド（DBカラムなし）
- **HeaderMenuFieldDesign** - ヘッダーメニュー。PageFrameで参照先フレーム指定
- **SidebarMenuFieldDesign** - サイドバーメニュー。PageFrameで参照先フレーム指定
- **ContextMenuFieldDesign** - 右クリックメニュー

### チャート系フィールド（外部ライブラリ、DBカラムなし）
- **ApexChartFieldDesign** - 棒グラフ/折れ線/面グラフ/散布図/ヒートマップ。SearchCondition.ModuleNameでデータ元指定、CategoryFieldでX軸、Series（ChartSeries）でY軸のNumberField系列を指定。SeriesType: Bar/Line/Area/Scatter/Heatmap
- **ApexRadialChartFieldDesign** - 円グラフ/ドーナツ/極座標チャート。SearchCondition.ModuleNameでデータ元指定、CategoryFieldでカテゴリ、SeriesFieldで値のNumberFieldを1つ指定。SeriesType: Donut/Pie/PolarArea

### その他
- **OptimisticLockingFieldDesign** - 楽観ロック。DbColumn, IncrementVersion
- **ProCodeFieldDesign** - カスタムBlazorコンポーネント。ProCodeComponent

---

## フィールド共通基底クラス

TypeFullNameが定義されているクラスは抽象クラスで持たれるためにJSONから復元するときに元の型が必要です。TypeFullNameは必ず入れてください。

### FieldDesignBase（全フィールド共通）
| プロパティ | 型 | 説明 |
|---|---|---|
| Name | string | フィールド名（Module内で一意、PascalCase） |
| IgnoreModification | bool | 変更検出を無視するか |
| TypeFullName | string | フィールド型の完全修飾名（必須） |

### ValueFieldDesignBase（値フィールド共通、FieldDesignBase継承）
| プロパティ | 型 | 説明 |
|---|---|---|
| DisplayName | string | 表示名 |
| IsRequired | bool | 必須入力か |
| OnDataChanged | string | 値変更時のスクリプトイベント名 |

### DbValueFieldDesignBase（DB連携フィールド共通、ValueFieldDesignBase継承）
| プロパティ | 型 | 説明 |
|---|---|---|
| DbColumn | string | DBカラム名（snake_case） |
| IsUpdateProtected | bool | 更新保護（既存データの変更不可）。サーバーサイドでもチェックが入り、API経由での更新も防止。 |
| IsSimpleSearchParameter | bool | 簡易検索パラメータ。trueにすると一覧ページの簡易検索バーにこのフィールドが含まれる。 |
| OnSearchDataChanged | string | 検索条件変更時のスクリプトイベント名 |

### IsUpdateProtected と IsViewOnly の違い（重要）
- IsUpdateProtected: **フィールド定義**のプロパティ。サーバーサイドでもチェックが入り、API経由での更新も防止される。DBから値を取得するだけのフィールドに適する。
- IsViewOnly: **レイアウト要素**（FieldLayoutDesign, GridLayoutDesign, ListElement等）のプロパティ。表示上の読み取り専用。フィールド定義（Fields配列内）に書いてもデシリアライズ時に無視される。

---

## 検索条件 (SearchCondition)

ListField, LinkField, SelectField等で使用。他のモジュールとのデータ関連付けに重要。

### SearchCondition
| プロパティ | 型 | 説明 |
|---|---|---|
| ModuleName | string | 検索対象のモジュール名 |
| LimitCount | int? | 取得件数上限 |
| SelectFields | List<string> | 取得するフィールド名リスト（空は全フィールド） |
| SortConditions | List<SortCondition> | ソート条件 |
| Condition | MatchConditionBase | 検索条件 |

### Variable記法
Variableは「フィールド名.メンバ名」の形式です。通常は「フィールド名.Value」を使用します。
- 例: Id.Value, Name.Value, CategoryId.Value

### FieldVariableMatchCondition（フィールド同士の比較）
| プロパティ | 型 | 説明 |
|---|---|---|
| SearchTargetVariable | string | 検索対象のフィールド変数（検索先モジュールのフィールド） |
| Comparison | string | Equal/NotEqual/LessThan/LessThanOrEqual/GreaterThan/GreaterThanOrEqual/Like/In/NotIn |
| Variable | string | 比較元のフィールド変数（現在のモジュールのフィールド） |

### FieldValueMatchCondition（フィールドと固定値の比較）
| プロパティ | 型 | 説明 |
|---|---|---|
| SearchTargetVariable | string | 検索対象のフィールド変数 |
| Comparison | string | 比較演算子 |
| Value | MultiTypeValue | 比較値（StringValue, DecimalValue, BooleanValue等） |

### MultiMatchCondition（複合条件）
| プロパティ | 型 | 説明 |
|---|---|---|
| IsOrMatch | bool | true: OR結合、false: AND結合 |
| IsNot | bool | 条件を反転 |
| Children | List<MatchConditionBase> | 子条件のリスト |

### ヘッダ明細パターンの例
ヘッダモジュール側のListFieldで明細モジュールを表示する場合:
- SearchCondition.ModuleName: 明細モジュール名
- Condition: FieldVariableMatchCondition
  - SearchTargetVariable: ""明細のヘッダID保持フィールド名.Value""（検索先＝明細側）
  - Comparison: ""Equal""
  - Variable: ""Id.Value""（現在のモジュール＝ヘッダ側のID）

### MultiTypeValue（FieldValueMatchConditionのValueに使用）
値の型に応じたクラスを使用:
- StringValue: { ""Value"": ""文字列"", ""TypeFullName"": ""..."" }
- DecimalValue: { ""Value"": 123, ""TypeFullName"": ""..."" }
- BooleanValue: { ""Value"": true, ""TypeFullName"": ""..."" }
- DateOnlyValue: { ""Value"": ""2024-01-01"", ""TypeFullName"": ""..."" }
- NullValue: { ""TypeFullName"": ""..."" }

---

## 権限条件 (UserWriteCondition等)

ModuleMatchConditionの構造:
| プロパティ | 型 | 説明 |
|---|---|---|
| ModuleName | string | 条件評価に使用するモジュール名 |
| Condition | MatchConditionBase | 条件（通常MultiMatchCondition） |

UserWriteCondition, UserReadCondition: ユーザーの権限に基づくアクセス制御
DataWriteCondition, DataReadCondition: データの内容に基づくアクセス制御
条件が空（ModuleName空、Condition null）の場合はアクセス制限なし。

---

## フィールド追加時の推奨デフォルト値

新しくDB連携フィールドを追加する場合:
- DbColumn: フィールド名をsnake_caseに変換（例: ProductName → product_name）
- IsUpdateProtected: false
- IsRequired: false
- IsSimpleSearchParameter: false

新しくIdFieldを追加する場合:
- DbColumn: ""id""
- IsManualInput: false

新しくLinkFieldを追加する場合:
- SearchCondition.LimitCount: 50
- ValueVariable: ""Id.Value""
- DisplayTextVariable: ""Name.Value""（参照先モジュールに応じて変更）

## フィールドの型変更後の注意

フィールドの型や用途を変更した際に、`IsRequired: true` 等の既存設定が残ったままにならないよう注意。
直接編集されなくなったフィールド（LinkFieldNames経由の表示専用等）に必須チェックが効いてしまい、ValidateInput()が失敗する原因になる。
";
    }
}
