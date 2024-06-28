using PdfSharp.Fonts;

namespace LowCodeSamples.Server.Services
{
  public class CustomFontResolver : IFontResolver
  {
    public byte[] GetFont(string faceName)
    {
      var path = Path.Combine(SystemConfig.Instance.FontFileDirectory, faceName + ".ttf");
      if (!File.Exists(path)) path = Path.Combine(SystemConfig.Instance.FontFileDirectory, "NotoSansJP.ttf");
      return File.ReadAllBytes(path);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
      familyName = familyName.Replace(" ", "");
      if (isBold) familyName += "#b";
      return new FontResolverInfo(familyName.Replace(" ", ""));
    }
  }
}
