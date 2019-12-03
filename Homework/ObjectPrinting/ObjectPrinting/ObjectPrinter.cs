namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        internal const int DefaultSerialiseDepth = 50;

        public static PrintingConfig<T> For<T>(int serialiseDepth = DefaultSerialiseDepth) =>
            new PrintingConfig<T>(serialiseDepth);
    }
}