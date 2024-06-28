using Blazor.DynamicJS;
using Microsoft.JSInterop;

namespace IGSample.Client.Shared.Services
{
  public class Logger : Codeer.LowCode.Blazor.RequestInterfaces.ILogger
  {
    readonly IJSRuntime _jsRuntime;
    readonly ToasterEx _toaster;

    public Logger(IJSRuntime js, ToasterEx toaster)
    {
      _jsRuntime = js;
      _toaster = toaster;
    }

    public async Task Log(string message)
    {
      await using var js = await _jsRuntime.CreateDymaicRuntimeAsync();
      await js.GetWindow().console.log(message, new JSAsync());
    }

    public async Task Warn(string message)
    {
      _toaster.Warn(message);
      await using var js = await _jsRuntime.CreateDymaicRuntimeAsync();
      await js.GetWindow().console.warn(message, new JSAsync());
    }

    public async Task Error(string message)
    {
      _toaster.Error(message);
      await using var js = await _jsRuntime.CreateDymaicRuntimeAsync();
      await js.GetWindow().console.error(message, new JSAsync());
    }
  }
}
