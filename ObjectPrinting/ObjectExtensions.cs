namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static PrintingConfig<T> GetObjectPrinter<T>(this T _)
        {
            return ObjectPrinter.For<T>();
        }

        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}
