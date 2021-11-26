using ObjectPrinting.Solved.PrintingConfiguration;

namespace ObjectPrinting.Solved
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}