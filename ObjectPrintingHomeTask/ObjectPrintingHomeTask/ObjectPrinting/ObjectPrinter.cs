using ObjectPrintingHomeTask.Config;

namespace ObjectPrintingHomeTask.ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>() => new PrintingConfig<T>();
    }
}
