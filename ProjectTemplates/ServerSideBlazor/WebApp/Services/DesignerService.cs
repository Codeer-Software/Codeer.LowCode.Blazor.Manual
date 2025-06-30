using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Data;
using WebApp.Client.Shared.Services;
using WebApp.Server.Shared;

namespace WebApp.Services
{
    static class DesignerService
    {
        static object _sync = new();
        static DesignData _designData = new();

        internal static DesignData GetDesignData()
        {
            lock (_sync)
            {
                _designData = DesignDataFileManager.GetDesignData(SystemConfig.Instance.DesignFileDirectory, _designData);
                DbAccessor.ClearTableDefinitionCache();
                return _designData;
            }
        }

        internal static DesignData GetDesignDataForFront(ModuleData? currentUser)
        {
            var data = GetDesignData().Clone();
            data.PageFrames = data.ResolvePageFrames(new PageLinkUrlResolver(), currentUser).ToPageFrameDesigns();
            return data;
        }

        internal static MemoryStream? GetResource(string resourcePath)
            => DesignDataFileManager.GetResource(SystemConfig.Instance.DesignFileDirectory, resourcePath);
    }
}
