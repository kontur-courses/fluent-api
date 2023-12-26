namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static IBaseConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}