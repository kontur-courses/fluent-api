namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj, int serialiseDepth = 30) =>
            ObjectPrinter.For<T>(serialiseDepth).PrintToString(obj);
    }
}