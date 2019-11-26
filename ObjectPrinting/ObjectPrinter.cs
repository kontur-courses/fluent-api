namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>(T obj) => new PrintingConfig<T>(obj);
    }
}