namespace ObjectPrinting
{
    public static class StringExtensions
    {
        public static string SafetySubstring(this string str, int start, int length)
        {
            if (start + length > str.Length)
                return str;
            return str.Substring(start, length);
        }
    }
}