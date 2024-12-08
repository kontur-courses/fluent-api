using ObjectPrinting.Configs;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ObjectPrinting.Tests")]
namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}