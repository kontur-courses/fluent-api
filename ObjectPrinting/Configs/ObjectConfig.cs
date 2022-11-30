using ObjectPrinting.ObjectConfiguration;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting
{
    public class ObjectConfig
    {
        public static IObjectConfiguration<T> For<T>() =>
            new ObjectConfiguration<T>();
    }
}