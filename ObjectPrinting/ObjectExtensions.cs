namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string ToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().ToString(obj);
        }
    }
}