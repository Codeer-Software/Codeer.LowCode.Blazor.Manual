using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using UglyToad.PdfPig;
using Codeer.LowCode.Blazor.Repository.Data;
using System.Text.Json;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using System.Text;

namespace WebApp.Services.AI
{
    public static class AITextAnalyzeService
    {
        public static async Task<ModuleData> FileToDataAsync(ModuleDataIO moduleDataIO, string? moduleName, string? fileName, MemoryStream memoryStream)
        {
            var text = await ExtractText(fileName ?? string.Empty, memoryStream);
            return await TextToDataAsync(moduleDataIO, moduleName, text);
        }

        public static async Task<ModuleData> TextToDataAsync(ModuleDataIO moduleDataIO, string? moduleName, string text)
        {
            var json = await DocumentAnalysisByText(DesignerService.GetDesignData().Modules, moduleName ?? string.Empty, text);
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
Please select and return the most likely match from the provided options.
If no match is found, return ""???"".
The response will be interpreted by a program, so absolutely include no additional information.
Do not, under any circumstances, enclose the answer or respond with any acknowledgments, such as ""Understood.""
"),
                    new UserChatMessage(string.Join(Environment.NewLine, candidates.Keys)),
                    new UserChatMessage(text),
                ]);
            return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }

        static async Task<string> DocumentAnalysisByText(IModuleDesigns moduleDesigns, string moduleName, string text)
        {
            var config = SystemConfig.Instance.AISettings;

            var azureClient = new AzureOpenAIClient(
                new Uri(config.OpenAIEndPoint),
                new ApiKeyCredential(config.OpenAIKey));
            var chatClient = azureClient.GetChatClient(config.ChatModel);

            var completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(@"
You are responsible for extracting specific data from the text. I will provide instructions on the data to retrieve along with the text. Please return the data in JSON format.
The instructions will include field names, and if applicable, put supplementary names and types inside parentheses (supplementary name: type). Use the field names in the JSON output. There may be arrays; in that case, specify them recursively as [{instructions for the child element fields}].
Since the response will be used in a program, provide only the JSON. Absolutely do not include anything like ""Understood"" or ""```json""."),
                    new UserChatMessage(CreateJsonExplanation(moduleDesigns, moduleName)),
                    new UserChatMessage(text),
                ]);
            return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }

        static async Task<string> ExtractText(string fileName, MemoryStream stream)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".pdf":
                    return ExtractTextFromPdf(stream);
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return await ExtractTextFromImage(stream);
            }
            throw LowCodeException.Create("Invalid file type");
        }

        static async Task<string> ExtractTextFromImage(MemoryStream stream)
        {
            var config = SystemConfig.Instance.AISettings;
            var client = new DocumentAnalysisClient(new Uri(config.DocumentAnalysisEndPoint), new AzureKeyCredential(config.DocumentAnalysisKey));
            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", stream);
            return string.Join(Environment.NewLine, operation.Value.Pages.SelectMany(e => e.Lines).Select(e => e.Content));
        }

        static string ExtractTextFromPdf(MemoryStream stream)
        {
            try
            {
                using var document = PdfDocument.Open(stream);
                var sb = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    var words = page.GetWords();
                    double previousY = -1; 

                    foreach (var word in words)
                    {
                        var currentY = word.BoundingBox.Bottom;

                        if (previousY != -1 && Math.Abs(previousY - currentY) > 5)
                        {
                            sb.AppendLine();
                        }

                        sb.Append(word.Text + " ");
                        previousY = currentY;
                    }

                    sb.AppendLine();
                }
                return sb.ToString();
            }
            catch
            {
                throw LowCodeException.Create("Invalid file data");
            }
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
            foreach (var field in moduleDesign.Fields)
            {
                var info = new List<string>([GetJsonType(field)]);
                if (field is IDisplayName diplayName) info.Add(diplayName.DisplayName);

                var explanation = $"{field.Name}({string.Join(", ", info)})";
                if (field is ListFieldDesign listFieldDesign)
                {
                    explanation += $"[{CreateJsonExplanation(moduleDesigns, listFieldDesign.SearchCondition.ModuleName)}]";
                }
                list.Add(explanation);
            }
            return string.Join(",", list);
        }

        static string GetJsonType(FieldDesignBase design)
        {
            if (design is BooleanFieldDesign booleanData) return "Boolean";
            else if (design is NumberFieldDesign numberData) return "Number";
            else if (design is ListFieldDesign ListData) return "Array";
            return "String";
        }
    }
}
