using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Extras.Designs;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Repository.Data;
using LowCodeSamples.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LowCodeSamples.Server.Controllers
{
    [ApiController]
    [Route("api/ai_text_analyze")]
    public class AITextAnalyzeController : ControllerBase
    {
        readonly DataService _dataService;

        public AITextAnalyzeController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        static int _count = 0;
        static DateTime _lastTime = DateTime.Now;
        static void Check()
        {
            _count++;
            if (24 < (DateTime.Now - _lastTime).TotalHours)
            {
                _lastTime = DateTime.Now;
                _count = 0;
            }
            if (1000 < _count) throw new Exception("1日1000回までです。");
        }

        [HttpPost("file")]
        public async Task<ModuleData> FileToDataAsync(string? moduleName, string? fieldName, string? fileName)
        {
            Check();

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
            Check();

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
