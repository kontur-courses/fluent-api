namespace ObjectPrinting.Utilits.Strings
{
    internal static class StringExtensions
    {
        public static string TrimEnd(this string line, string toTrim)
        {
            var length = line.Length;
            for (var i = 1; i <= toTrim.Length; i++)
            {
                var currentChar = toTrim[^i];
                while (length > 0 && line[length - 1] == currentChar)
                {
                    length -= 1;
                }
            }
            return line.Substring(0, length);
        }
    }
}
