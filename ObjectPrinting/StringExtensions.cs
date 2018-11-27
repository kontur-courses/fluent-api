namespace ObjectPrinting
{
    public static class StringExtensions
    {
        public static string Cut(this string str, int len)
        {
            var substrLen = len < str.Length ? len : str.Length;
            return str.Substring(0, substrLen);
        }
    }
}