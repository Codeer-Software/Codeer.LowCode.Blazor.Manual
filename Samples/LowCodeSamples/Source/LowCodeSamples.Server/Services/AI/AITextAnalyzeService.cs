using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Design.Samples.AIDocumentAnalyzer;
using LowCodeSamples.Server.Services;
using LowCodeSamples.Server.Services.AI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LowCodeSamples.Server.Services.AI
{
    public static class AITextAnalyzeService
    {
        public static async Task<ModuleData> FileToDataAsync(ModuleDataIO moduleDataIO, string? moduleName, string? fieldName, string? fileName, MemoryStream memoryStream)
        {
            var text = await ExtractTextFromFile(memoryStream);
            return await TextToDataAsync(moduleDataIO, moduleName, fieldName, text, $"テキストは[{fileName}]を解析したjsonです。");
        }

        public static async Task<ModuleData> TextToDataAsync(ModuleDataIO moduleDataIO, string? moduleName, string? fieldName, string text)
        => await TextToDataAsync(moduleDataIO, moduleName, fieldName, text, string.Empty);


        public static async Task<ModuleData> TextToDataAsync(ModuleDataIO moduleDataIO, string? moduleName, string? fieldName, string text, string source)
        {
            var json = await DocumentAnalysisByText(DesignerService.GetDesignData().Modules, moduleName ?? string.Empty, fieldName ?? string.Empty, text, source);
            return await CreateModule(DesignerService.GetDesignData().Modules, moduleName ?? string.Empty,
                new FieldCandidatesResolver(moduleDataIO, DesignerService.GetDesignData().Modules, FindCandidatesByAI),
                JsonSerializer.Deserialize<JsonElement>(json));
        }

        static async Task<string?> FindCandidatesByAI(Dictionary<string, string> candidates, string text)
        {
            var config = SystemConfig.Instance.AISettings;

            var azureClient = new AzureOpenAIClient(
                new Uri(config.OpenAIEndPoint),
                new ApiKeyCredential(config.OpenAIKey));
            var chatClient = azureClient.GetChatClient(config.ChatModel);

            var completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(@"
提供された選択肢から最も可能性の高い一致を1つ選び、その値のみを返してください。
一致が見つからない場合は ""???"" を返してください。
応答はプログラムによって解釈されるため、絶対に追加情報を含めないでください。
答えを囲んだり、""了解しました"" のような確認の文言で返答したりしないでください。
"),
                    new UserChatMessage(string.Join(Environment.NewLine, candidates.Keys)),
                    new UserChatMessage(text),
                ]);
            return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }

        static async Task<string> DocumentAnalysisByText(IModuleDesigns moduleDesigns, string moduleName, string? fieldName, string text, string source)
        {
            var config = SystemConfig.Instance.AISettings;

            var mod = moduleDesigns.Find(moduleName);
            var field = mod?.Fields.FirstOrDefault(e => e.Name == fieldName) as AITextAnalyzerFieldDesign;
            if (field == null) throw LowCodeException.Create($"Invalid Field {moduleName}.{fieldName}");

            var azureClient = new AzureOpenAIClient(
                new Uri(config.OpenAIEndPoint),
                new ApiKeyCredential(config.OpenAIKey));
            var chatClient = azureClient.GetChatClient(config.ChatModel);

            var completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(field.Remarks + @$"
あなたはテキストから特定のデータを抽出する役割を担います。
私が取得すべきデータの指示とテキストを提示します。
{source}
抽出結果は JSON 形式で返してください。
指示には項目名が含まれ、必要に応じて補助名や型を括弧内に（補助名: 型）の形式で示します。
JSON 出力では、その項目名をキーとして使用してください。
配列が含まれる場合は、子要素の項目指示を再帰的に [{{子要素の項目指示}}] の形で指定します。
この応答はプログラムで使用されるため、JSON のみを返してください。
""了解しました"" や ""```json"" のような文言は絶対に含めないでください。"),
                    new UserChatMessage(CreateJsonExplanation(moduleDesigns, moduleName)),
                    new UserChatMessage(text),
                ]);
            return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }

        static async Task<string> ExtractTextFromFile(MemoryStream stream)
        {
            var cfg = SystemConfig.Instance.AISettings;
            var client = new DocumentAnalysisClient(
                new Uri(cfg.DocumentAnalysisEndPoint),
                new AzureKeyCredential(cfg.DocumentAnalysisKey));

            if (stream.CanSeek) stream.Position = 0;

            var options = new AnalyzeDocumentOptions { Locale = "ja" }; // locale
            var op = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", stream, options);
            var doc = op.Value;

            double R(double v) => Math.Round(v, 3);

            var pagesOut = new List<object>(doc.Pages.Count);

            foreach (var p in doc.Pages)
            {
                var linesOut = new List<object>(p.Lines.Count);

                foreach (var line in p.Lines)
                {
                    var xs = line.BoundingPolygon.Select(pt => pt.X);
                    var ys = line.BoundingPolygon.Select(pt => pt.Y);
                    double left = xs.Min(), right = xs.Max();
                    double bottom = ys.Min(), top = ys.Max();

                    linesOut.Add(new
                    {
                        text = line.Content,
                        rect = new
                        {
                            t = R(top),
                            l = R(left),
                            b = R(bottom),
                            r = R(right)
                        }
                    });
                }
                pagesOut.Add(new { lines = linesOut });
            }

            return JsonSerializer.Serialize(
                pagesOut,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
        }

        static async Task<ModuleData> CreateModule(IModuleDesigns moduleDesigns, string moduleName, FieldCandidatesResolver candidateCache, JsonElement root)
        {
            var moduleDesign = moduleDesigns.Find(moduleName);
            if (moduleDesign == null) throw LowCodeException.Create($"Invalid Module {moduleName}");

            var moduleData = new ModuleData { Name = moduleDesign.Name };
            foreach (var element in root.EnumerateObject())
            {
                var fieldDesign = moduleDesign.Fields.FirstOrDefault(e => e.Name == element.Name);
                if (fieldDesign == null) continue;
                var value = GetValue(element.Value);
                var data = fieldDesign.CreateData();

                try
                {
                    if (data is BooleanFieldData booleanData) booleanData.Value = Convert.ToBoolean(value);
                    else if (data is IdFieldData idData) idData.Value = Convert.ToString(value);
                    else if (data is TextFieldData textData) textData.Value = Convert.ToString(value);
                    else if (data is NumberFieldData numberData) numberData.Value = Convert.ToDecimal(value);
                    else if (data is DateFieldData dateData) dateData.Value = DateOnly.FromDateTime(Convert.ToDateTime(value));
                    else if (data is DateTimeFieldData dateTimeData) dateTimeData.Value = Convert.ToDateTime(value);
                    else if (data is TimeFieldData TimeData) TimeData.Value = TimeOnly.Parse(value?.ToString() ?? string.Empty);
                    else if (data is ListFieldData ListData)
                    {
                        var childModuleName = ((ListFieldDesign)fieldDesign).SearchCondition.ModuleName;
                        foreach (var e in element.Value.EnumerateArray())
                        {
                            ListData.Children.Add(await CreateModule(moduleDesigns, childModuleName, candidateCache, e));
                        }
                    }
                    else if (data is SelectFieldData selectData) await candidateCache.GetSelectValue(moduleName, (SelectFieldDesign)fieldDesign, value?.ToString() ?? string.Empty, selectData);
                    else if (data is LinkFieldData linkData) await candidateCache.GetLinkValue(moduleName, (LinkFieldDesign)fieldDesign, value?.ToString() ?? string.Empty, linkData);
                    else continue;
                }
                catch
                {
                    continue;
                }
                moduleData.Fields.Add(element.Name, data);
            }
            return moduleData;
        }

        static object? GetValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) return element.GetString();
            else if (element.ValueKind == JsonValueKind.Number) return element.GetDecimal();
            else if (element.ValueKind == JsonValueKind.True) return true;
            else if (element.ValueKind == JsonValueKind.False) return false;
            return element;
        }

        static string CreateJsonExplanation(IModuleDesigns moduleDesigns, string moduleName)
        {
            var moduleDesign = moduleDesigns.Find(moduleName);
            if (moduleDesign == null) throw LowCodeException.Create($"Invalid Module {moduleName}");

            var list = new List<string>();
            foreach (var field in moduleDesign.Fields.Where(e => IsSupportedType(e)))
            {
                var info = new List<string>([GetJsonType(field)]);
                if (field is IDisplayName diplayName && !string.IsNullOrEmpty(diplayName.DisplayName)) info.Add(diplayName.DisplayName);
                var explanation = $"{field.Name}({string.Join(", ", info)})";
                if (field is ListFieldDesign listFieldDesign)
                {
                    explanation += $"[{CreateJsonExplanation(moduleDesigns, listFieldDesign.SearchCondition.ModuleName)}]";
                }
                list.Add(explanation);
            }
            return string.Join(",", list);
        }

        static bool IsSupportedType(FieldDesignBase? e) =>
            e is BooleanFieldDesign
             or IdFieldDesign
             or TextFieldDesign
             or NumberFieldDesign
             or DateFieldDesign
             or DateTimeFieldDesign
             or TimeFieldDesign
             or ListFieldDesign
             or SelectFieldDesign
             or LinkFieldDesign;

        static string GetJsonType(FieldDesignBase design)
        {
            if (design is BooleanFieldDesign booleanData) return "Boolean";
            else if (design is NumberFieldDesign numberData) return "Number";
            else if (design is ListFieldDesign ListData) return "Array";
            return "String";
        }
    }
}
