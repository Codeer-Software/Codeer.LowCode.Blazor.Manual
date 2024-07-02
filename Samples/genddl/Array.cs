namespace genddl
{
    internal static class Arrays
    {
        public static string GetOrDefault(this string[] a, int index)
        {
            if (index < a.Length) return a[index];
            return "";
        }
    }
}
