namespace ObjectPrintingHomeTask.ObjectPrinting
{
    internal static class ObjectPrinterExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}
