using ClosedXML.Excel;
using Codeer.LowCode.Blazor.RequestInterfaces;
using Excel.Report.PDF;
using System.Globalization;

namespace WebApp.Client.Shared.Services
{
    public class LocalizeService
    {
        Dictionary<string, string> _dic = new();

        public string Localize(string text)
            => _dic.TryGetValue(text, out var localizedText) ? localizedText : text;

        public static LocalizeService? Create(string localizeResourceName, MemoryStream? mem)
        {
            var service = new LocalizeService();
            var lowName = localizeResourceName.ToLower();
            if (lowName.EndsWith(".xlsx")) service._dic = FromExcel(mem);
            if (!service._dic.Any()) return null;
            return service;
        }

        static Dictionary<string, string> FromExcel(MemoryStream? mem)
        {
            if (mem == null) return new();
            using var book = new XLWorkbook(mem);

            var texts = book.Worksheet(1).ReadAllTexts();
            if (texts.Count < 2) return new();

            var index = texts.First().IndexOf(CultureInfo.CurrentCulture.Name);
            if (index < 1) index = 1;

            Dictionary<string, string> dic = new();
            foreach (var row in texts.Skip(1))
            {
                if (row.Count < index) continue;
                var key = row[0].Trim();
                var text = row[index].Trim();
                dic[key] = text;
            }
            return dic;
        }
    }

    public static class LocalizeServiceHelper
    {
        public static async Task<LocalizeService?> CreateLocalizeService(this IAppInfoService app)
        {
            var _design = app.GetDesignData();
            if (string.IsNullOrEmpty(_design.AppSettings.LocalizeResourcePath)) return null;
            return LocalizeService.Create(_design.AppSettings.LocalizeResourcePath, await app.GetResourceAsync(_design.AppSettings.LocalizeResourcePath));
        }
    }
}
