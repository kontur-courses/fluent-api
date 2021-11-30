using ObjectPrinting.Configs;

namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>() => new();
    }
}