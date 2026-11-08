using Microsoft.JSInterop;

namespace LowCodeSamples.Client.Shared.Services
{
    // スクリプトからクリップボードに文字列を書く
    public class Clipboard
    {
        readonly IJSRuntime _js;

        public Clipboard(IJSRuntime js) => _js = js;

        public async Task Copy(string text) => await _js.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}
