namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static CustomSerializablePrintingConfig<T> For<T>() => new CustomSerializablePrintingConfig<T>();
    }
}