using ObjectPrinting.Configs;

namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
        
        public static PrintingConfig<T> For<T>(T serializedObject)
        {
            return new PrintingConfig<T>(serializedObject);
        }
    }
}