namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static PrintingConfig<T> GetObjectPrinter<T>(this T obj) => ObjectPrinter.For(obj);
    }
}
