using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}