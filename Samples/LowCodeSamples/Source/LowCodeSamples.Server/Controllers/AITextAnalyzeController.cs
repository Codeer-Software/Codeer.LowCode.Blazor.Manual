using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Extras.Server.AI;
using Codeer.LowCode.Blazor.Repository.Data;
using Microsoft.AspNetCore.Mvc;
using LowCodeSamples.Server.Services;

namespace LowCodeSamples.Server.Controllers
{
    //AITextAnalyzerField の受け口。ロジックは Extras.Server (AITextAnalyzeService) にあり、ここは結線だけを持つ
    //moduleName / fieldName の AITextAnalyzerField が今のユーザーに見えるときだけ解析する (アプリアクセス条件・モジュールの UserRead・フィールド読取権限)。補足指示 (Remarks) はデザインから
    [ApiController]
    [Route("api/ai_text_analyze")]
    public class AITextAnalyzeController : ControllerBase, IAsyncDisposable
    {
        readonly DataService _dataService;

        public AITextAnalyzeController(DataService dataService)
            => _dataService = dataService;

        public async ValueTask DisposeAsync()
            => await _dataService.DisposeAsync();

        //デモサイト用: AI 解析の呼び出しは 1 日 1000 回まで
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
            Check();

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
