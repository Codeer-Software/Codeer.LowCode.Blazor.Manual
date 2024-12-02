using Codeer.LowCode.Blazor.Components.Dialog;
using Microsoft.JSInterop;
using System.Reflection;

namespace AccessSample.Client.Shared.Services
{
    public class UIService : Codeer.LowCode.Blazor.Components.UIService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ToasterEx _toaster;

        public UIService(
            ModuleDialogService moduleDialogService,
            MessageBoxService messageBoxService,
             IJSRuntime JSRuntime,
             ToasterEx toaster
        ) : base(moduleDialogService, messageBoxService)
        {
            _jsRuntime = JSRuntime;
            _toaster = toaster;
        }

        public override async Task<bool> DownloadFile(MemoryStream stream, string name)
        {
            await Download(_jsRuntime, name, stream.ToArray());
            return true;
        }

        public override async Task NotifySuccess(string message)
        {
            await Task.CompletedTask;
            _toaster.Success(message);
        }

        public override async Task NotifyError(string message)
        {
            await Task.CompletedTask;
            _toaster.Error(message);
        }

        static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".txt", "text/plain"},
            {".csv", "text/csv"},
            {".json", "application/json"},
            {".pdf", "application/pdf"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/png"},
            {".gif", "image/gif"},
            {".bmp", "image/bmp"},
            {".tiff", "image/tiff"},
            {".svg", "image/svg+xml"},
            {".html", "text/html"},
            {".htm", "text/html"},
            {".xml", "application/xml"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".zip", "application/zip"},
            {".rar", "application/x-rar-compressed"},
            {".7z", "application/x-7z-compressed"},
            {".tar", "application/x-tar"},
            {".gz", "application/gzip"},
            {".mp3", "audio/mpeg"},
            {".wav", "audio/wav"},
            {".ogg", "audio/ogg"},
            {".mp4", "video/mp4"},
            {".avi", "video/x-msvideo"},
            {".mov", "video/quicktime"},
            {".wmv", "video/x-ms-wmv"},
            {".flv", "video/x-flv"},
            {".mkv", "video/x-matroska"},
            {".ico", "image/x-icon"},
        };

        static async Task Download(IJSRuntime jsRuntime, string fileName, byte[] bin)
        {
            using var stream = new MemoryStream(bin);
            using var streamReference = new DotNetStreamReference(stream);
            await using var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/{Assembly.GetExecutingAssembly().GetName().Name}/interop/download.js");
            var mimeType = MimeTypes.TryGetValue(fileName, out var value) ? value : string.Empty;
            await module.InvokeVoidAsync("download", streamReference, mimeType, fileName);
        }
    }
}
