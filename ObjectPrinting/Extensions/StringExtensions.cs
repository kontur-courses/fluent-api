namespace ObjectPrinting.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string text, int maxLength, string truncationSuffix = "")
        {
            return text.Length > maxLength
                ? text[..maxLength] + truncationSuffix
                : text;
        }
    }
}