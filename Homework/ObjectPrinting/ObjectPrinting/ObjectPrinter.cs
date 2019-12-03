namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>(int serialiseDepth = 30) => new PrintingConfig<T>(serialiseDepth);
    }
}