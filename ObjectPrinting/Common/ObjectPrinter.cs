using ObjectPrinting.Common.Configs;

namespace ObjectPrinting.Common
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}