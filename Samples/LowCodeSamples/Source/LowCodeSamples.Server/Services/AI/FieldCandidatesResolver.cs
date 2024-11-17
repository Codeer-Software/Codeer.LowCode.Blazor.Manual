using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DataLogic;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace LowCodeSamples.Server.Services.AI
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

        public async Task<string?> GetSelectValue(string moduleName, SelectFieldDesign design, string text)
        {
            if (design.Candidates.Any())
            {
                foreach (var e in design.Candidates)
                {
                    var sp = e.Split(",").Select(y => y.Trim()).ToList();
                    if (sp.Contains(text)) return sp.Last();
                }
                return null;
            }
            return await GetValue(moduleName, design.Name, design.SearchCondition.ModuleName, design.DisplayTextVariable, design.ValueVariable, text);
        }

        public async Task<string?> GetLinkValue(string moduleName, LinkFieldDesign design, string text)
            => await GetValue(moduleName, design.Name, design.SearchCondition.ModuleName, design.DisplayTextVariable, design.ValueVariable, text);

        async Task<string?> GetValue(string moduleName, string fieldName, string targetModule, string displayTextVariable, string valueVariable, string text)
        {
            var fieldCandidates = _fieldCandidatesList.FirstOrDefault(e => e.ModuleName == moduleName && e.FieldName == fieldName);
            if (fieldCandidates == null)
            {
                var mod = _modules.Find(targetModule);
                if (mod == null) return null;

                var displayTextName = new VariableName(displayTextVariable);
                var valueName = new VariableName(valueVariable);

                var condition = new SearchCondition { ModuleName = mod.Name };
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
            }
            var item = fieldCandidates.Candidates.FirstOrDefault(e => e.Key == text || e.Value == text);

            if (string.IsNullOrEmpty(item.Value))
            {
                var key = await _findCandidatesByAI(fieldCandidates.Candidates, text);
                return fieldCandidates.Candidates.TryGetValue(key ?? string.Empty, out var value) ? value : null;
            }
            return item.Value;
        }
    }
}
