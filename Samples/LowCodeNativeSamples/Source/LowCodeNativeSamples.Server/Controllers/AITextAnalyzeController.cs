using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Extras.Designs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LowCodeNativeSamples.Server.Services;

namespace LowCodeNativeSamples.Server.Controllers
{
    [Authorize, AutoValidateAntiforgeryToken]
    [ApiController]
    [Route("api/ai_text_analyze")]
    public class AITextAnalyzeController : ControllerBase
    {
        readonly DataService _dataService;

        public AITextAnalyzeController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        [HttpPost("file")]
        public async Task<ModuleData> FileToDataAsync(string? moduleName, string? fieldName, string? fileName)
        {
            var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            try
            {
                return await new AITextAnalyzeService(SystemConfig.Instance.AISettings).FileToDataAsync(
                    _dataService.ModuleDataIO, DesignerService.GetDesignData().Modules,
                    moduleName ?? string.Empty, GetRemarks(moduleName, fieldName), fileName, memoryStream);
            }
            catch
            {
                throw new Exception("AI analysis failed. Retrying may succeed.");
            }
        }

        [HttpPost("text")]
        public async Task<ModuleData> TextToDataAsync(string? moduleName, string? fieldName, [FromForm] string? text)
        {
            try
            {
                return await new AITextAnalyzeService(SystemConfig.Instance.AISettings).TextToDataAsync(
                    _dataService.ModuleDataIO, DesignerService.GetDesignData().Modules,
                    moduleName ?? string.Empty, GetRemarks(moduleName, fieldName), text ?? string.Empty);
            }
            catch
            {
                throw new Exception("AI analysis failed. Retrying may succeed.");
            }
        }

        static string GetRemarks(string? moduleName, string? fieldName)
        {
            var mod = DesignerService.GetDesignData().Modules.Find(moduleName ?? string.Empty);
            var field = mod?.Fields.FirstOrDefault(e => e.Name == fieldName) as AITextAnalyzerFieldDesign;
            if (field == null) throw LowCodeException.Create($"Invalid Field {moduleName}.{fieldName}");
            return field.Remarks;
        }
    }
}
