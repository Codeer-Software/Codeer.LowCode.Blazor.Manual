using Codeer.LowCode.Blazor.Json;

namespace IGSample.Client.Shared.ScriptObjects
{
  public class WebApiResult
  {
    public JsonObject JsonObject { get; set; } = new();
    public int StatusCode { get; set; }
  }
}
