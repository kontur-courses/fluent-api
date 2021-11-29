using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>() => new();
    }
}