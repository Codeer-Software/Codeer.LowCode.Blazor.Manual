using Codeer.LowCode.Blazor.Repository.Data;
using LowCodeSamples.Server.Services;
using LowCodeSamples.Server.Services.AI;
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
        public async Task<ModuleData> FileToDataAsync(string? moduleName, string? fileName)
        {
            Check();

            var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            try
            {
                return await AITextAnalyzeService.FileToDataAsync(_dataService.ModuleDataIO, moduleName, fileName, memoryStream);
            }
            catch
            {
                throw new Exception("AI analysis failed. Retrying may succeed.");
            }
        }

        [HttpPost("text")]
        public async Task<ModuleData> TextToDataAsync(string? moduleName, [FromForm] string? text)
        {
            Check();

            try
            {
                return await AITextAnalyzeService.TextToDataAsync(_dataService.ModuleDataIO, moduleName, text ?? string.Empty);
            }
            catch
            {
                throw new Exception("AI analysis failed. Retrying may succeed.");
            }
        }
    }
}
