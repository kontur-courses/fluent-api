using ObjectPrintingTask.PrintingConfiguration;

namespace ObjectPrintingTask
{
    public class ObjectPrinter
    {
        public static Printer<T> For<T>()
        {
            return new Printer<T>(new PrintingConfig<T>());
        }
    }
}