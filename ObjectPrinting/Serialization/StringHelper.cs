namespace ObjectPrinting.Serialization
{
    public static class StringHelper
    {
        public static string Trim(string value, int length)
        {
            if (value == null)
                return null;

            return value.Length <= length
                ? value
                : value[..length];
        }
    }
}