namespace ObjectPrinting
{
    public static class PrinterExtensions
    {
        public static PrintingConfig<T> CreatePrinter<T>(this T instance)
        {
            return ObjectPrinter.For<T>();
        }
    }
}