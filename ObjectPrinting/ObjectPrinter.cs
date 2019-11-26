namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> SetUp<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}