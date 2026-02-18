using System.ClientModel;
using System.Reflection;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor.Designer.Config;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.SystemSettings;
using OpenAI.Chat;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib.AI
{
    public static class DbNameCreator
    {
        public static async Task CreateDbNames(DesignerSettings settings, ModuleDesign module, bool isAll)
        {
            var dataSource = settings.DataSources.FirstOrDefault(e => e.Name == module.DataSourceName) ?? new() { DataSourceType = DataSourceType.PostgreSQL };

            var lines = new List<string>();
            lines.Add($"DBType:{dataSource.DataSourceType}");
            lines.Add($"[{module.Name}]");
            foreach (var field in module.Fields)
            {
                if (field.Name.Contains(".")) continue;
                var columns = field.GetType().GetProperties().Select(e => new { prop = e, attr = e.GetCustomAttribute<DbColumnAttribute>() }).Where(e => e.attr != null).ToList();
                if (!columns.Any()) continue;
                if (columns.Count == 1)
                {
                    lines.Add($"[{field.Name}]");
                }
                else
                {
                    foreach (var e in columns)
                    {
                        lines.Add($"[{field.Name}.{e.attr!.DataMember}]");
                    }
                }
            }
            var inputText = string.Join(Environment.NewLine, lines);

            var config = AISettings.Instance;

            var azureClient = new AzureOpenAIClient(
                new Uri(config.OpenAIEndPoint),
                new ApiKeyCredential(config.OpenAIKey));
            var chatClient = azureClient.GetChatClient(config.ChatModel);

            var completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(@"
You are an AI that generates column names for a database. I will provide class names and variable names corresponding to a program. Based on this input, generate table names from class names and column names from variable names.

In some cases, a variable may correspond to multiple database columns. In such cases, specify the internal variables as Variable.Size, Variable.Value, etc. If a variable name is ""OptimisticLocking,"" it indicates usage for optimistic locking. Please assign the following based on the database type:

SQLServer: row_version
Oracle: ORA_ROWSCN
PostgreSQL: xmin
SQLite: version
MySQL: version
Optimize the table and column names according to database characteristics:
Note that anytime you need to translate other languages into English, translate it by its meaning, NOT by its pronunciations.

SQLServer: Keep names as-is; remove any unsupported characters
Oracle: Use uppercase English in snake_case
PostgreSQL: Use lowercase English in snake_case
SQLite: Use lowercase English in snake_case
MySQL: Use lowercase English in snake_case
I will provide input in the following format:

DBType: [Database Type]
[ClassName]
(VariableName1)
(VariableName2)

Return your response in the following format:
DBType: [Database Type]
[ClassName: TableName]
(VariableName1: ColumnName1)
(VariableName2: ColumnName2)

The returned string will be used in subsequent programming, so include no extraneous information—do not add text like ""Understood"" or any custom delimiters.
"),
                    new UserChatMessage(inputText),
                ]);

            var resultText = completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

            var nameInfoList = resultText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1)
                .Select(e => e.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "").Split(":").Select(y => y.Trim()).ToArray())
                .Where(e => e.Length == 2);

            var table = nameInfoList.FirstOrDefault();
            if (table == null) return;

            if (isAll || string.IsNullOrEmpty(module.DbTable)) module.DbTable = table[1];

            foreach (var nameInfo in nameInfoList.Skip(1))
            {
                var fieldName = nameInfo[0].Split(".");

                var field = module.Fields.FirstOrDefault(e => e.Name == fieldName.First());
                if (field == null) continue;

                var columns = field.GetType().GetProperties().Select(e => new { prop = e, attr = e.GetCustomAttribute<DbColumnAttribute>() }).Where(e => e.attr != null).ToList();

                if (!columns.Any()) continue;

                if (columns.Count == 1)
                {
                    var prop = columns.First().prop;
                    if (isAll || string.IsNullOrEmpty(prop.GetValue(field)?.ToString())) prop.SetValue(field, nameInfo[1]);
                }
                else if (1 < fieldName.Length)
                {
                    var prop = columns.FirstOrDefault(e => e.attr!.DataMember == fieldName.Last())?.prop;
                    if (isAll || string.IsNullOrEmpty(prop?.GetValue(field)?.ToString())) prop!.SetValue(field, nameInfo[1]);
                }
            }
        }
    }
}
