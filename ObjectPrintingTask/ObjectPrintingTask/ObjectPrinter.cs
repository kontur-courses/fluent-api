using ObjectPrintingTask.PrintingConfiguration;

namespace ObjectPrintingTask
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}