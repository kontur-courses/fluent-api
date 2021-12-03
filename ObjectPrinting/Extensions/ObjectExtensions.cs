namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, IPrintingConfig<T> printingConfig)
        {
            return printingConfig.PrintToString(obj);
        }
    }
}