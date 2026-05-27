using LowCodeApp.WinForms.Client.Shared.Services;

namespace LowCodeApp.WinForms.Client.Shared.ScriptObjects
{
    public class Toaster
    {
        readonly ToasterEx? _core;
        public Toaster(ToasterEx core) => _core = core;
        public void Success(string s) => _core?.Success(s);
        public void Info(string s) => _core?.Info(s);
        public void Warn(string s) => _core?.Warn(s);
        public void Error(string s) => _core?.Error(s);
    }
}
