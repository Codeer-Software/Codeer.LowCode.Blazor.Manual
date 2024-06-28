using Blazor.DynamicJS;
using Codeer.LowCode.Blazor.Components.Dialog;
using Microsoft.JSInterop;

namespace CustomLayoutSample.Client.Shared.Services
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

        static async Task Download(DynamicJSRuntime _js, string fileName, byte[] bin)
        {
            var window = _js.GetWindow();
            var blob = await new JSSyntax(window.Blob).NewAsync(new[] { bin }, new { type = "application/zip" });
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
