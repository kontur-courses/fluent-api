namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj) => ObjectPrinter.For<T>().PrintToString(obj);
    }
}