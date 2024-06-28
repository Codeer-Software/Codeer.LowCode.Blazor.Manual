using Sotsera.Blazor.Toaster;

namespace CustomLayoutSample.Client.Shared.Services
{
    public class ToasterEx
    {
        readonly IToaster _toaster;

        public ToasterEx(IToaster toaster) => _toaster = toaster;

        public void Clear() => _toaster.Clear();

        public void Success(string s)
        {
            _toaster.Clear();
            _toaster.Success(s);
        }

        public void Warn(string s) => _toaster.Warning(s);

        public void Error(string s)
        {
            _toaster.Error(s, null, config =>
            {
                config.MaximumOpacity = 100;
                config.VisibleStateDuration = 1000 * 30;
                config.ShowTransitionDuration = 10;
                config.HideTransitionDuration = 500;
            });
        }
    }
}
