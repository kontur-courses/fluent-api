using ObjectPrinting.Contexts;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static ConfigPrintingContext<T> For<T>() => new(new PrintingConfig());
    }
}