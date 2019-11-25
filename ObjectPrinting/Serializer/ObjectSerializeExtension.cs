namespace ObjectPrinting
{
    public static class ObjectSerializeExtension
    {
        public static PrintingConfig<T> Serialize<T>(this T obj)
        {
            return ObjectPrinter.For<T>();
        }
    }
}