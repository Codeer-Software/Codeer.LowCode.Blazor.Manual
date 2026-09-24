using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Repository.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LowCodeNativeSamples.Server.Services;

namespace LowCodeNativeSamples.Server.Controllers
{
    //AITextAnalyzerField の受け口。ロジックは Extras.Server (AITextAnalyzeService) にあり、ここは結線だけを持つ
    //moduleName / fieldName の AITextAnalyzerField が今のユーザーに見えるときだけ解析する (アプリアクセス条件・モジュールの UserRead・フィールド読取権限)。補足指示 (Remarks) はデザインから
    [Authorize, AutoValidateAntiforgeryToken]
    [ApiController]
    [Route("api/ai_text_analyze")]
    public class AITextAnalyzeController : ControllerBase, IAsyncDisposable
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
                return await new AITextAnalyzeService(SystemConfig.Instance.AISettings).AnalyzeFileAsync(
                    _dataService.ModuleDataIO, DesignerService.GetDesignData().Modules, moduleName, fieldName, fileName, memoryStream);
            }
            catch (LowCodeException)
            {
                //権限・デザインの拒否はそのまま返す
                throw;
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
                return await new AITextAnalyzeService(SystemConfig.Instance.AISettings).AnalyzeTextAsync(
                    _dataService.ModuleDataIO, DesignerService.GetDesignData().Modules, moduleName, fieldName, text);
            }
            catch (LowCodeException)
            {
                throw;
            }
            catch
            {
                throw new Exception("AI analysis failed. Retrying may succeed.");
            }
        }
    }
}
