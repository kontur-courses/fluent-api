namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        public static PrintingConfig<T> For<T>(Config config)
        {
            return new PrintingConfig<T>(config);
        }
    }
}