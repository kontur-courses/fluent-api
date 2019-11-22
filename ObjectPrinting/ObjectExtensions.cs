namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static PrintingConfig<T> Printing<T>(this T obj)
        {
            return new PrintingConfig<T>(obj);
        }
        
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}