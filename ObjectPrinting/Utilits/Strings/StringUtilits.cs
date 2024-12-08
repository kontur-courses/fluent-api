namespace ObjectPrinting.Utilits.Strings
{
    internal static class StringUtilits
    {
        public static string GetIdentation(int nestingLevel)
            => new string('\t', nestingLevel);
    }
}
