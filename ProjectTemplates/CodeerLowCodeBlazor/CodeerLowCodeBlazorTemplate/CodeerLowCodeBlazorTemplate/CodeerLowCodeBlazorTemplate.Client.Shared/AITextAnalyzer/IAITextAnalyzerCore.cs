using Codeer.LowCode.Blazor.Repository.Data;

namespace CodeerLowCodeBlazorTemplate.Client.Shared.AITextAnalyzer
{
    public interface IAITextAnalyzerCore
    {
        Task<ModuleData?> FileToModuleDataAsync(string moduleName, string fieldName, string fileName, StreamContent content);
        Task<ModuleData?> TextToModuleDataAsync(string moduleName, string fieldName, string text);
    }
}
