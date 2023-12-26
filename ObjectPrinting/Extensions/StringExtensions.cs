namespace ObjectPrinting.Extensions
{
    public static class StringExtensions
    {
        internal static string Truncate(this string text, int maxLength, string truncationSuffix = "")
        {
            return text.Length > maxLength
                ? text[..maxLength] + truncationSuffix
                : text;
        }
    }
}