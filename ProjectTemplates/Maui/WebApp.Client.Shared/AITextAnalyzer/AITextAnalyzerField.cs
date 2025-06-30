using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Script;
using Design.Samples.AIDocumentAnalyzer;
using WebApp.Client.Shared.AITextAnalyzer;

namespace WebApp.Client.Shared.Samples.AIDocumentAnalyzer
{
    public class AITextAnalyzerField(AITextAnalyzerFieldDesign design)
        : FieldBase<AITextAnalyzerFieldDesign>(design)
    {
        [ScriptHide]
        public Func<Task> OnDataImportCompletedAsync { get; set; } = async () => await Task.CompletedTask;

        private readonly AITextAnalyzerFieldDesign _design = design;

        public override bool IsModified => false;

        [ScriptHide]
        public override FieldDataBase? GetData() => null;

        [ScriptHide]
        public override FieldSubmitData GetSubmitData() => new();

        [ScriptHide]
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        [ScriptHide]
        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        [ScriptHide]
        public override bool ValidateInput() => true;

        public async Task SetDataByFileAsync(IAITextAnalyzerCore core, string fileName, StreamContent content)
        {
            var mod = await core.FileToModuleDataAsync(Module.Design.Name, fileName, content);
            if (mod == null) return;
            await Module.SetDataAsync(mod);

            await Module!.ExecuteScriptAsync(Design.DataImportCompleted);
            await OnDataImportCompletedAsync();
        }

        public async Task SetDataByTextAsync(IAITextAnalyzerCore core, string text)
        {
            var mod = await core.TextToModuleDataAsync(Module.Design.Name, text);
            if (mod == null) return;
            await Module.SetDataAsync(mod);

            await Module!.ExecuteScriptAsync(Design.DataImportCompleted);
            await OnDataImportCompletedAsync();
        }
    }
}
