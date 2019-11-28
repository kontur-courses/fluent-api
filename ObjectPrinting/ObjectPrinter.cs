namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        public static string PrintToString<T>(T obj, PrintingConfig<T> configuration)
        {
            return configuration.GetStringRepresentation(obj);
        }
    }
}