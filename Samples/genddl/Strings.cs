using System.Globalization;

namespace genddl
{
    internal static class Strings
    {
        public static string PascalCase(string snakeCaseName)
        {
            if (string.IsNullOrEmpty(snakeCaseName)) return snakeCaseName;

            var words = snakeCaseName.Split('_');
            if (words.Length == 1)
            {
                if (string.IsNullOrEmpty(words[0])) return string.Empty;
                var first = CultureInfo.InvariantCulture.TextInfo.ToUpper(words[0].Substring(0, 1));
                if (words[0].Length == 1) return first;
                return first + words[0].Substring(1);
            }

            var capitalizedWords = words.Select(word =>
                CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLower()));
            return string.Concat(capitalizedWords);
        }
    }
}
