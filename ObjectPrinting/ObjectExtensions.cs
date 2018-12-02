namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
            where T : new()
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}