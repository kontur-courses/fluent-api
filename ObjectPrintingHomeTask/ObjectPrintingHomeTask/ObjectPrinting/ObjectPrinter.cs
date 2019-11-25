using ObjectPrintingHomeTask.Config;

namespace ObjectPrintingHomeTask.ObjectPrinting
{
    internal class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}
