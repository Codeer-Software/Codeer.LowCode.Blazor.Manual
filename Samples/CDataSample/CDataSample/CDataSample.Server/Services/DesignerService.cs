using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Json;

namespace CDataSample.Server.Services
{
    internal static class DesignerService
    {
        static object _sync = new();
        static DesignData _designData = new();

        internal static DesignData GetDesignData()
        {
            lock (_sync)
            {
                _designData = DesignDataFileManager.GetDesignData(SystemConfig.Instance.DesignFileDirectory, _designData);
                return _designData;
            }
        }

        internal static DesignData GetDesignDataForFront()
        {
            var data = GetDesignData().JsonClone();
            data.Modules = data.ModulesWithoutDataSourceInfo;
            return data;
        }

        internal static MemoryStream? GetResource(string resourcePath)
            => DesignDataFileManager.GetResource(SystemConfig.Instance.DesignFileDirectory, resourcePath);
    }
}
