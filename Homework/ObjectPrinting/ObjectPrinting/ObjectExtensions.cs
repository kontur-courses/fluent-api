namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj, int serialiseDepth = ObjectPrinter.DefaultSerialiseDepth) =>
            ObjectPrinter.For<T>(serialiseDepth).PrintToString(obj);
    }
}