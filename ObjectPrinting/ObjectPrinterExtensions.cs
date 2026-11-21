
namespace ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        public static string Print<T>(this  T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}
