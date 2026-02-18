using Codeer.LowCode.Blazor.Repository.Data;
using CodeerLowCodeBlazorTemplate.Server.Services;
using CodeerLowCodeBlazorTemplate.Server.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace CodeerLowCodeBlazorTemplate.Server.Controllers
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

        [HttpPost("file")]
        public async Task<ModuleData> FileToDataAsync(string? moduleName, string? fieldName, string? fileName)
        {
            var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            try
            {
                return await AITextAnalyzeService.FileToDataAsync(_dataService.ModuleDataIO, moduleName, fieldName, fileName, memoryStream);
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
                return await AITextAnalyzeService.TextToDataAsync(_dataService.ModuleDataIO, moduleName, fieldName, text ?? string.Empty);
            }
            catch
            {
                throw new Exception("AI analysis failed. Retrying may succeed.");
            }
        }
    }
}
