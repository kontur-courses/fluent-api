namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static IBasicConfigurator<T> For<T>()
        {
            return new ObjectConfigurator<T>();
        }
    }
}