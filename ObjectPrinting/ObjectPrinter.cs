namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static IBasicConfigurator<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}