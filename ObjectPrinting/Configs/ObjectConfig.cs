using ObjectPrinting.BasicConfigurator;
using ObjectPrinting.BasicConfigurator.Implementation;

namespace ObjectPrinting
{
    public class ObjectConfig
    {
        public static IBasicConfigurator<T> For<T>() =>
            new ObjectConfigurator<T>();
    }
}