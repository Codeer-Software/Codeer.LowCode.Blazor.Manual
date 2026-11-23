using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Extras.Designs;
using Codeer.LowCode.Blazor.Extras.Server.Excel;
using Codeer.LowCode.Blazor.Extras.Server.Jobs;
using Codeer.LowCode.Blazor.Jobs;
using LowCodeSamples.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace LowCodeSamples.Server.Jobs
{
    /// <summary>
    /// デザイナで作った Job (Jobs/名前.job.cs) を画面のボタン (JobTriggerField) から動かす。
    /// デモサイトは Web サーバーのプロセスの中で動かす (プロセス内実行) だけ。コマンドラインからの起動 (dotnet xxx.dll job 名前) は
    /// Cookie テンプレート (Starter) の Jobs/JobRuntimeHost を参照。
    /// </summary>
    public static class JobRuntimeHost
    {
        //Job のスクリプトから Excel を使えるようにする
        static JobRuntimeHost() => ExcelApi.UseInJobScripts();

        /// <summary>動かせるジョブの名前 (デザイナで作った Job)。</summary>
        public static IReadOnlyList<string> Names => JobExecutor.GetJobNames(DesignerService.GetDesignData());

        /// <summary>画面のボタンから起動されたジョブを動かす。input は画面の内容 (ジョブの args[0])、userId はボタンを押したユーザー。失敗は例外 (文言は画面に出る)。</summary>
        public static async Task<JobRuntimeResult> RunFromScreenAsync(string name, string input, string userId, JobTriggerProcessType processType)
        {
            var logger = JobTriggerTracker.LoggerFactory?.CreateLogger(typeof(JobRuntimeHost)) ?? NullLogger.Instance;
            await using var data = new DataService();
            var result = await JobExecutor.RunAsync(new JobExecutionContext
            {
                ModuleDataIO = data.ModuleDataIO,
                DesignData = DesignerService.GetDesignData(),
                GetResource = DesignerService.GetResource,
                InvokerUserId = userId,
                FileStorage = data.TemporaryFileManager,
            }, name, [input], new JobScriptLogger(logger));
            if (!result.Succeeded)
            {
                //失敗の文言は画面に出る。Fail の理由と本体の文言 (検証・権限など) はそのまま、それ以外は固定の文言 (原文はスクリプトの位置や DB の例外を含むのでログだけに出す)
                logger.LogError("ジョブ {Name} が失敗しました: {Error}", name, result.Error);
                throw new InvalidOperationException(result.Exception is LowCodeException ? result.Exception.Message : "ジョブでエラーが発生しました。");
            }
            return new JobRuntimeResult { Message = result.Message, ResultJson = result.ResultJson };
        }
    }
}
