using Blazor.DynamicJS;
using Codeer.LowCode.Blazor.Components.Dialog;
using Microsoft.JSInterop;

namespace LowCodeSamples.Client.Shared.Services
{
    public class UIService : Codeer.LowCode.Blazor.Components.UIService
    {
        IJSRuntime _jsRuntime;
        ToasterEx _toaster;
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
            await using var js = await _jsRuntime.CreateDymaicRuntimeAsync();
            await Download(js, name, stream.ToArray());
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

        static async Task Download(DynamicJSRuntime _js, string fileName, byte[] bin)
        {
            var mimeType = MimeTypes.TryGetValue(fileName, out var value) ? value : string.Empty;

            var window = _js.GetWindow();
            var blob = await new JSSyntax(window.Blob).NewAsync(new[] { bin }, new { type = mimeType });
            var url = await window.URL.createObjectURL(blob, new JSAsync<string>());
            var anchorElement = await window.document.createElement("a", new JSAsync<dynamic>());
            await new JSSyntax(anchorElement.href).SetValueAsync(url);
            await new JSSyntax(anchorElement.download).SetValueAsync(fileName ?? "");
            await anchorElement.click(new JSAsync());
            await anchorElement.remove(new JSAsync());
            await window.URL.revokeObjectURL(url, new JSAsync());
        }
    }
}
