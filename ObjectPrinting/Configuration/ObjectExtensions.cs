namespace ObjectPrinting.Configuration
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return new ObjectPrinter<T>(new PrintingConfig<T>()).PrintToString(obj);
        }
    }
}