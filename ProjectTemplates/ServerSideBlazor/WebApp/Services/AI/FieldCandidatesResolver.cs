using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.DataLogic;
using Codeer.LowCode.Blazor.Repository.Data;

namespace WebApp.Services.AI
{
    public class FieldCandidatesResolver
    {
        class FieldCandidates
        {
            internal string ModuleName { get; set; } = string.Empty;
            internal string FieldName { get; set; } = string.Empty;
            internal Dictionary<string, string> Candidates { get; } = new Dictionary<string, string>();
        }

        ModuleDataIO _moduleDataIO;
        IModuleDesigns _modules;
        List<FieldCandidates> _fieldCandidatesList = new List<FieldCandidates>();
        Func<Dictionary<string, string>, string, Task<string?>> _findCandidatesByAI;

        public FieldCandidatesResolver(ModuleDataIO moduleDataIO, IModuleDesigns modules, Func<Dictionary<string, string>, string, Task<string?>> findCandidatesByAI)
        {
            _moduleDataIO = moduleDataIO;
            _modules = modules;
            _findCandidatesByAI = findCandidatesByAI;
        }

        public async Task GetSelectValue(string moduleName, SelectFieldDesign design, string text, SelectFieldData data)
        {
            if (design.Candidates.Any())
            {
                foreach (var e in design.Candidates)
                {
                    var sp = e.Split(",").Select(y => y.Trim()).ToList();
                    if (sp.Contains(text)) data.Value = sp.Last();
                }
                return;
            }
            var result = await GetValue(moduleName, design.Name, design.SearchCondition.ModuleName, design.DisplayTextVariable, design.ValueVariable, text);
            data.DisplayText = result.DisplayText ?? string.Empty;
            data.Value = result.Value;
        }

        public async Task GetLinkValue(string moduleName, LinkFieldDesign design, string text, LinkFieldData data)
        {
            var result = await GetValue(moduleName, design.Name, design.SearchCondition.ModuleName, design.DisplayTextVariable, design.ValueVariable, text);
            data.DisplayText = result.DisplayText ?? string.Empty;
            data.Value = result.Value;
        }

        async Task<(string DisplayText, string? Value)> GetValue(string moduleName, string fieldName, string targetModule, string displayTextVariable, string valueVariable, string text)
        {
            var fieldCandidates = await GetFieldCandidates(moduleName, fieldName, targetModule, displayTextVariable, valueVariable);

            // Since AI is performing data analysis, it is possible that values are passed not only by Key but also by Value.
            var item = fieldCandidates.Candidates.FirstOrDefault(e => e.Key == text || e.Value == text);

            if (!string.IsNullOrEmpty(item.Value)) return (item.Key, item.Value);

            var key = (await _findCandidatesByAI(fieldCandidates.Candidates, text)) ?? string.Empty;
            return fieldCandidates.Candidates.TryGetValue(key, out var value2) ? (key, value2) : (key, null);
        }

        async Task<FieldCandidates> GetFieldCandidates(string moduleName, string fieldName, string targetModule, string displayTextVariable, string valueVariable)
        {
            var fieldCandidates = _fieldCandidatesList.FirstOrDefault(e => e.ModuleName == moduleName && e.FieldName == fieldName);
            if (fieldCandidates != null) return fieldCandidates; 

            var mod = _modules.Find(targetModule);
            if (mod == null) return fieldCandidates = new FieldCandidates { ModuleName = string.Empty, FieldName = string.Empty };

            var condition = new SearchCondition { ModuleName = mod.Name };
            var displayTextName = new VariableName(displayTextVariable);
            var valueName = new VariableName(valueVariable);

            condition.SelectFields.Add(displayTextName.FieldName.FullName);
            condition.SelectFields.Add(valueName.FieldName.FullName);
            var ret = await _moduleDataIO.GetListAsync(condition, 0);

            fieldCandidates = new FieldCandidates { ModuleName = moduleName, FieldName = targetModule };

            foreach (var e in ret.Items)
            {
                if (!e.TryGetVariableValue(displayTextName, out var displayText) ||
                    !e.TryGetVariableValue(valueName, out var value)) continue;

                fieldCandidates.Candidates[displayText?.ToString() ?? string.Empty] = value?.ToString() ?? string.Empty;
            }

            _fieldCandidatesList.Add(fieldCandidates);
            
            return fieldCandidates;
        }
    }
}
