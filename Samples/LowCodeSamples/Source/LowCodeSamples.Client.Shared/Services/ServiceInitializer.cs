using Codeer.LowCode.Blazor.Components.AppParts.Loading;
using Codeer.LowCode.Blazor.Components.AppParts.PageFrame;
using Codeer.LowCode.Blazor.Components.Dialog;
using Codeer.LowCode.Blazor.Extras.Approval;
using Codeer.LowCode.Blazor.Extras.Fields;
using Codeer.LowCode.Blazor.Extras.Mail;
using Codeer.LowCode.Blazor.Extras.ScriptObjects;
using Codeer.LowCode.Blazor.Extras.Services;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Sotsera.Blazor.Toaster.Core.Models;
using System.Globalization;

namespace LowCodeSamples.Client.Shared.Services
{
    public static class ServiceInitializer
    {
        public static void AddSharedServices(this IServiceCollection services)
        {
            //Extras の組み込みサービスが使うエンドポイント。URL はアプリ(Controller を持つ側)の持ち物なのでここで一元定義する
            MailTransport.SendMailEndPoint = "/api/mail";
            MailTransport.BulkSearchMailEndPoint = "/api/mail/bulk_search";
            MailTransport.PreviewMailEndPoint = "/api/mail/preview";
            MailTransport.BulkPreviewMailEndPoint = "/api/mail/bulk_preview";
            ApprovalTransport.EndPointBase = "/api/approval";
            Codeer.LowCode.Blazor.Extras.ScriptObjects.Excel.ConvertPdfEndPoint = "api/excel/pdf";
            AITextAnalyzerField.FileToModuleDataEndPoint = "/api/ai_text_analyze/file";
            AITextAnalyzerField.TextToModuleDataEndPoint = "/api/ai_text_analyze/text";
            BulkFileReader.ParseFileEndPoint = "/api/module_data/parse_file";
            BulkFileTransferService.ListFileByDataEndPoint = "/api/module_data/list_file_by_data";
            BulkFileTransferService.BulkSubmitEndPoint = "/api/module_data/bulk_submit";

            services.AddScoped<IAppInfoService, AppInfoService>();
            services.AddScoped<IModuleDataService, ModuleDataService>();
            services.AddScoped<IUIService, UIService>();
            services.AddScoped<Codeer.LowCode.Blazor.RequestInterfaces.Services>();
            services.AddScoped<ILogger, Logger>();
            services.AddSingleton<LoadingService>();
            services.AddToaster(config =>
            {
                config.PositionClass = Defaults.Classes.Position.BottomRight;
                config.MaximumOpacity = 100;
                config.VisibleStateDuration = 1000 * 5;
                config.ShowTransitionDuration = 10;
                config.HideTransitionDuration = 500;
            });
            services.AddScoped<IToastService, ToastService>();
            services.AddScoped<IHttpService, HttpService>();

            var cultureName = CultureInfo.CurrentCulture.Name;
            if (cultureName == "ja") cultureName = "ja-JP";
            var cultureInfo = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
