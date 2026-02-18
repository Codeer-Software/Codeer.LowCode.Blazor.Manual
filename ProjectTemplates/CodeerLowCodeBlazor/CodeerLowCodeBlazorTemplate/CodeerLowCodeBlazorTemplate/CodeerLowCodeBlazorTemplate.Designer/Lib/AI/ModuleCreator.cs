using System.ClientModel;
using System.IO;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Extensibility;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.SystemSettings;
using OpenAI.Chat;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib.AI
{
    public class ModuleCreator : IAIChat
    {
        List<ChatMessage> _chatHistory = new();
        AzureOpenAIClient _azureClient;
        ChatClient _chatClient;
        DesignerEnvironment _designerEnvironment;

        public ModuleCreator(DesignerEnvironment designerEnvironment, AISettings settings)
        {
            _designerEnvironment = designerEnvironment;
            _azureClient = new AzureOpenAIClient(
                new Uri(settings.OpenAIEndPoint),
                new ApiKeyCredential(settings.OpenAIKey));

            _chatClient = _azureClient.GetChatClient(settings.ChatModel);
            Clear();
        }

        public void Clear()
        {
            _chatHistory.Add(new SystemChatMessage(@"
あなたはローコードのアプリ設計者です。
このローコードシステムの仕様は以下のものです。

# PageFrame
Webページのヘッダ、サイドバーの部分の定義です。
複数存在する場合があります。
あなたはこれを作る櫃ようはありません。
しかしサイドバーにモジュールを追加するときなどにどのPageFrameに追加するかをユーザーに選択させる必要があります。

# Module
## DBのテーブルとマッピングできます。
## Fieldを複数持ちます。Fieldには種類があり後述します。
## マッピングしたテーブルに対してCRUD操作ができます。
## 複数のデータを一覧表示する画面と一つのデータを詳細表示する画面を作ることができます。検索機能もあります。

# Field
## 以下の種類があります。他にもありますがあなたが設計/実装に使うのはこれらだけです。
- Id
- Boolean
- Text
- Number
- Select
- Link
- List
- File
- Date
- DateTime
- Time
### 主キーに対してIdフィールドを割り当てる場合、その変数名は必ず""Id""にしてください。ユーザーがフィールド名を日本語にしてほしいといっても主キーだけは""Id""にしてください。
### Linkは他のModuleを参照することができます。例えば、[請求書]Module で [顧客]Module を参照するなどです。
### Selectは選択肢を持ちます。直値で指定することもできますし、他のModuleを指定してその一覧を選択肢として表示することができます。
### Listは明細を表示するのに使います。明細表示するModuleを指定できます。明細となるModuleは通常は所有者のIdをIdフィールドもしくはLinkフィールドで持ちます。

# PageFrame名、Module名、Field名はC#でコンパイルが通る必要があります。
"));
            var designData = _designerEnvironment.GetDesignData();

            bool IsUseField(Type type) => new List<Type> {
                typeof(IdFieldDesign), typeof(BooleanFieldDesign), typeof(TextFieldDesign), typeof(SelectFieldDesign), typeof(LinkFieldDesign),
                typeof(ListFieldDesign), typeof(FileFieldDesign), typeof(DateFieldDesign), typeof(DateTimeFieldDesign), typeof(TimeFieldDesign)
                }.Any(e => e.IsAssignableFrom(type));

            var currentMods = new List<string>();
            foreach (var mod in designData.Modules.ToList())
            {
                currentMods.Add(mod.Name + "[" + string.Join(",", mod.Fields.Where(e => IsUseField(e.GetType())).Select(e => $"{e.GetType().Name.Replace("Design", "")} {e.Name}")) + "]");
            }

            var currentPageFrames = new List<string>();
            foreach (var pageFrame in designData.PageFrames.ToList())
            {
                currentPageFrames.Add(pageFrame.Name);
            }

            var dbTypes = string.Join(Environment.NewLine, _designerEnvironment.GetDesignerSettings().DataSources.Select(e => $"{e.Name} : {e.DataSourceType}"));
            if (string.IsNullOrEmpty(dbTypes))
            {
                dbTypes = string.Join(Environment.NewLine, Enum.GetValues<DataSourceType>().Select(e => $"{e} : {e}"));
            }

            _chatHistory.Add(new SystemChatMessage($@"
データソースは以下候補があります。一つならそれに決定です。複数の場合は必要に応じてユーザーに聞いてください。
データソース名 ; DBのタイプ です。ユーザーに問い合わせる場合はデータソース名を選択肢で表示してそこから選ばせてください。
{dbTypes}
"));

            _chatHistory.Add(new SystemChatMessage($@"
現在システム内には以下Moduleがすでに存在しています。Link、Select、Listでは必要に応じて参照してください。
{string.Join(Environment.NewLine, currentMods)}
"));

            _chatHistory.Add(new SystemChatMessage($@"
現在システム内には以下のPageFrameが存在しています。
{string.Join(Environment.NewLine, currentPageFrames)}
"));

            _chatHistory.Add(new SystemChatMessage(@"
今からユーザーがあなたに対してチャットでアプリの設計を相談します。
必要なModuleの構成を答えてください。
ユーザーとチャットで会話してModuleの構成を決定してください。
ここまでは登場するすべてのModuleの話をしてください。

最後にModuleを作成するかとうかたずねてください。
全部まとめて作ることもできますし、ピックアップして任意のモジュールだけ作成することもできます。
作成時は $Create コマンドを使います。

以下はプログラムが解釈して使うコマンドです。
このコマンドを返す場合は余計な情報は一切つけずにコマンドだけ返してください。
使えるコマンドは一回答につき一つまでです。
例えば $Create[ModuleA, ModuleB] と $DDL[ModuleA, ModuleB] を一つの回答で同時に返すことはできません。

ユーザーがモジュールの作成を求めた場合は、
$Create[ModuleA, ModuleB]
という回答を返してください。[]の中身はユーザーが作成すると指定したModuleの名前です。
ただし、DBのタイプが必要です。最初に渡したデータソースの情報から取得してください。一つならそれをつかって複数ある場合はユーザーに聞いてください。

ユーザーがDDLの作成を求めた場合は
ただしDDLを作成できるのは既存のModuleかもしくは$Createで作成されたModuleのみです。
$DDL[ModuleA, ModuleB]
という回答を返してください。[]の中身はユーザーが作成すると指定したModuleの名前です。
ただし、DBのタイプが必要です。最初に渡したデータソースの情報から取得してください。一つならそれをつかって複数ある場合はユーザーに聞いてください。

ユーザーがサイドバーへの追加求めたら
$SideBar[PageFrame, ModuleA, ModuleB]
という回答を返してください。[]の中身は第一引数はPageFrameで、第二引数以降はユーザーが作成すると指定したModuleの名前です。
ただし、PageFrameを特定する必要があります。PageFrameが複数ある場合はユーザーに聞いてください。一つの場合はそれを使ってください。

ユーザーがヘッダへの追加求めたら
$Header[ModuleA, ModuleB]
という回答を返してください。[]の中身は第一引数はPageFrameで、第二引数以降はユーザーが作成すると指定したModuleの名前です。
ただし、PageFrameを特定する必要があります。PageFrameが複数ある場合はユーザーに聞いてください。一つの場合はそれを使ってください。
"));
        }

        public async Task<string> ProcessMessage(string message)
        {
            var aiResponse = await ExecuteNormalChat(message);

            var ret = GetCommandAndModules(aiResponse);
            switch (ret.Command)
            {
                case "$Create":
                    return await ExecuteCreateCommand(ret.Modules);
                case "$DDL":
                    return ExecuteDDLCommand(ret.Modules);
                case "$SideBar":
                    return ExecuteSideBarCommand(ret.Modules);
                case "$Header":
                    return ExecuteHeaderCommand(ret.Modules);
                default:
                    return aiResponse;
            }
        }

        static (string Command, List<string> Modules) GetCommandAndModules(string aiResponse)
        {
            var commandPrefixes = new[] { "$Create", "$DDL", "$SideBar", "$Header" };

            foreach (var commandPrefix in commandPrefixes)
            {
                if (!aiResponse.Contains(commandPrefix)) continue;

                var startIndex = aiResponse.IndexOf(commandPrefix);
                var endIndex = aiResponse.IndexOf("]", startIndex);
                if (endIndex == -1) continue;

                var commandCoure = aiResponse.Substring(startIndex, endIndex - startIndex + 1).Replace(commandPrefix, "");
                var moduleNames = commandCoure.Split(new[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(e => e.Trim())
                                            .Where(e => !string.IsNullOrWhiteSpace(e))
                                            .ToList();
                return (commandPrefix, moduleNames);
            }

            return (string.Empty, []);
        }

        async Task<string> ExecuteNormalChat(string userMessage)
        {
            _chatHistory.Add(new UserChatMessage(userMessage));
            var result = await _chatClient.CompleteChatAsync(_chatHistory);
            var resultText = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
            _chatHistory.Add(new AssistantChatMessage(resultText));
            return resultText;
        }

        async Task<string> ExecuteCreateCommand(List<string> moduleNames)
        {
            var list = new List<List<string>>();
            var inner = new List<string>();
            foreach (var moduleName in moduleNames)
            {
                inner.Add(moduleName);
                if (3 <= inner.Count)
                {
                    list.Add(inner.ToList());
                    inner.Clear();
                }
            }
            if (inner.Any()) list.Add(inner.ToList());

            var ret = new List<string>();
            foreach (var modules in list)
            {
                ret.Add(await ExecuteCreateCommandCore(modules));
            }
            return string.Join(Environment.NewLine, ret);
        }

        async Task<string> ExecuteCreateCommandCore(List<string> moduleNames)
        {
            var moduleNamesText = string.Join(", ", moduleNames);

            var history = _chatHistory.ToList();

            history.Add(new SystemChatMessage(moduleNamesText + @"
をJsonで作成してください。

DBのテーブル名とカラム名はDBのタイプに応じて命名してください。

Fieldには複数DBのカラムが割り当たる場合があります。
その場合は
Field.Size、Field.Value などのように内部の変数を指定します。
OptimisticLockingというField名の場合、それは楽観ロックに使うものです。
それぞれ以下のものにしてください。
- SQLServer row_version
- Oracle ORA_ROWSCN
- PostgreSQL xmin
- SQLite version
- MySQL version

DBのテーブル名、カラム名はDBの特性を考慮して最適な名前にしてください。
英語と記載されている場合、他の言語でつけられた名前なら英訳してください。
- SQLServerはできるだけそのまま、利用できない文字は削除
- Oracleは英語で大文字のスネークケース
- PostgreSQLは英語で小文字のスネークケース
- SQLiteは英語で小文字のスネークケース
- MySQLは英語で小文字のスネークケース

JsonはModuleの配列にしてください。一つしかなくてもそれだけ入った配列にしてください。
Jsonの部分は以下でブロックとして区切ってください。
これを目印にその範囲をプログラムで解析してオブジェクトに変換します。
```json
```

- Module
{
  ""Name"": モジュール名,
  ""DataSourceName"": データソース名,
  ""DbTable"": テーブル名,
  ""Fields"": []
}

FieldのスキーマのTypeFullNameはすべて固定値です。
フォーマット指定中に出てくるVariableというのは Field名.Value という表記になります。Valueは固定文字です。
Field名の前には何もいりません。コンテキストで解決するのでモジュール名などはつける必要がありません。
Fieldは以下のいずれかを生成してください。
ここにないものは生成しないでください。

- Id
Idとなるデータを入れます。主キーです。
NameはId固定です。
{
  ""DbColumn"": カラム名,
  ""Name"": ""Id"",
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.IdFieldDesign""
}

- Text
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.TextFieldDesign""
},

- Number
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.NumberFieldDesign""
},

- Date
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.DateFieldDesign""
},

- DateTime
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.DateTimeFieldDesign""
},

- Time
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.TimeFieldDesign""
},

- Boolean
真理値
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.BooleanFieldDesign""
}

- File
ファイルです。DBのカラムを複数持ちます。
StorageNameは空にしておいてください。
{
  ""StorageName"": """",
  ""Name"": Field名,
  ""DbColumnFileName"": ファイル名を入れるカラム名,
  ""DbColumnFileSize"": ファイルサイズを入れるカラム名,
  ""DbColumnFileGuid"": ファイルのGUIDを入れるカラム名,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.FileFieldDesign""
},

- Link
他のModuleを指定することで、その候補の中から関連するデータをユーザーは選択します。
UIではポップアップで候補一覧が出ます。
例えば顧客一覧のModuleを指定するなどの使い方があります。
Variableは対象のModuleの直接のFieldを指定します。そのModuleのさらにリンク先のFieldを指定することはできません。
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""SearchCondition"": {
    ""ModuleName"": Module名
  },
  ""ValueVariable"": 値を取得するVariable,
  ""DisplayTextVariable"": 表示用のVariable,
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.LinkFieldDesign""
},

- Select
指定の候補の中からユーザーに選択させます。
Candidatesは""文字列"" もしくは ""表示文字,値"" の配列で指定できます。またLinkと同じくModuleを指定することでその一覧を表示させることもできます。
Variableは対象のModuleの直接のFieldを指定します。そのModuleのさらにリンク先のFieldを指定することはできません。
{
  ""DbColumn"": カラム名,
  ""Name"": Field名,
  ""SearchCondition"": {
    ""ModuleName"": Module名
  },
  ""ValueVariable"": 値を取得するVariable,
  ""DisplayTextVariable"": 表示用のVariable,

  ""Candidates"": [],
  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.SelectFieldDesign""
}

- List
Listです。指定したModuleの一覧を表示します。条件を指定することができて例えば親のIdに一致する行などが一般的な使い方です。
例としては
SearchTargetVariableにModuleNameで指定したModuleの持つParentIdフィールを指定して(つまりParentId.Valueが入る)、VariableにId.Valueが入るという使い方が一般的です。
DeleteTogether, CanCreate, CanUpdate, CanDeleteは明細で使う場合にはtrue、それ以外はfalseを指定してください。
{
  ""Name"": Field名,
  ""DeleteTogether"": true,
  ""CanCreate"": true,
  ""CanUpdate"": true,
  ""CanDelete"": true,

    ""SearchCondition"": {
        ""ModuleName"": Module名,
        ""Condition"": {
            ""IsOrMatch"": false,
            ""Children"": [
                {
                    ""SearchTargetVariable"": 検索対象のVariable,
                    ""Comparison"": ""Equal"",
                    ""Variable"": 自分のModule内のVariable,
                    ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Match.FieldVariableMatchCondition""
                }
            ],
            ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Match.MultiMatchCondition""
        }
    },

  ""TypeFullName"": ""Codeer.LowCode.Blazor.Repository.Design.ListFieldDesign""
}
"));
            history.Add(new UserChatMessage(moduleNamesText));
            var result = await _chatClient.CompleteChatAsync(history);
            var y = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            var index = y.IndexOf("```json");
            if (index < 0) return moduleNamesText + " の作成に失敗しました。";

            var index2 = y.IndexOf("```", index + "```json".Length);
            if (index2 < 0) return moduleNamesText + " の作成に失敗しました。";
            var json = y.Substring(index + "```json".Length, index2 - index - "```json".Length).Trim();

            var mods = JsonConverterEx.DeserializeObject<List<ModuleDesign>>(json)!;

            foreach (var mod in mods)
            {
                mod.CreateLayouts();
                File.WriteAllText(Path.Combine(Path.Combine(_designerEnvironment.CurrentFileDirectory, "Modules"), $"{mod.Name}.mod.json"),
                    JsonConverterEx.SerializeObject(mod));
            }
            return moduleNamesText + " を作成しました";
        }

        string ExecuteDDLCommand(List<string> moduleNames)
        {
            var mods = _designerEnvironment.GetDesignData().Modules;
            var dataSources = _designerEnvironment.GetDesignerSettings().DataSources;
            var ddls = new List<string>();
            foreach (var e in moduleNames)
            {
                var mod = mods.Find(e);
                if (mod == null) continue;

                if (ddls.Any()) ddls.Add("");
                ddls.AddRange(mod.CreateDDL(dataSources.FirstOrDefault(e => e.Name == mod.DataSourceName)?.DataSourceType ?? DataSourceType.PostgreSQL));
            }

            ddls.Insert(0, "");
            ddls.Insert(0, "DDLです");
            return string.Join(Environment.NewLine, ddls);
        }

        string ExecuteSideBarCommand(List<string> args)
            => AddLink(args, p => p.Left.IsVisible ? p.Left.Links : p.Right.Links, "サイドバー");

        string ExecuteHeaderCommand(List<string> args)
            => AddLink(args, p => p.Header.Links, "ヘッダ");

        string AddLink(List<string> args, Func<PageFrameDesign, List<PageLink>> get, string title)
        {
            var pageFrameName = args.FirstOrDefault() ?? string.Empty;
            var moduleNames = args.Skip(1).ToList();

            var designData = _designerEnvironment.GetDesignData();
            var pageFrame = designData.PageFrames.Find(pageFrameName);
            if (pageFrame == null) pageFrame = designData.PageFrames.ToList().FirstOrDefault();
            if (pageFrame == null) return "PageFrameが存在しません";

            var links = get(pageFrame);
            foreach (var module in moduleNames)
            {
                if (links.Any(e => e.Module == module)) continue;
                links.Add(new PageLink
                {
                    Module = module,
                    Title = module,
                });
            }
            var path = Path.Combine(_designerEnvironment.CurrentFileDirectory, "PageFrames", $"{pageFrame.Name}.frm.json");

            try
            {
                File.WriteAllText(path, JsonConverterEx.SerializeObject(pageFrame));
            }
            catch (Exception exp)
            {
                return "追加に失敗しました。\r\n" + exp.Message;
            }

            return string.Join(", ", moduleNames) + $"を{title}に追加しました";
        }
    }
}
