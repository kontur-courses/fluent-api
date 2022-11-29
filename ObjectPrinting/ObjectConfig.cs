namespace ObjectPrinting
{
    public class ObjectConfig
    {
        public static IBasicConfigurator<T> For<T>()
        {
            return new ObjectConfigurator<T>();
        }
    }
}