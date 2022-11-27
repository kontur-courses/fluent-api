namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToStringDefault<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}