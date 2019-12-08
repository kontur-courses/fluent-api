namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        internal const int DefaultSerialiseDepth = 50;
        internal const int DefaultSequencesMaxLength = 1000;

        public static PrintingConfig<T> For<T>(int serialiseDepth = DefaultSerialiseDepth,
                                               int sequencesMaxLength = DefaultSequencesMaxLength) =>
            new PrintingConfig<T>(serialiseDepth, sequencesMaxLength);
    }
}