namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T objectToPrint)
        {
            return ObjectPrinter.For<T>().PrintToString(objectToPrint);
        }
    }
}