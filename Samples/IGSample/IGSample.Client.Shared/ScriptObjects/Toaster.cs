using IGSample.Client.Shared.Services;

namespace IGSample.Client.Shared.ScriptObjects
{
  public class Toaster
  {
    readonly ToasterEx? _core;
    public Toaster(ToasterEx core) => _core = core;
    public void Success(string s) => _core?.Success(s);
    public void Warn(string s) => _core?.Warn(s);
    public void Error(string s) => _core?.Error(s);
  }
}
