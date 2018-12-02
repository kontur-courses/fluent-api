namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>(int maxDepth = 10)
        {
            return new PrintingConfig<T>(maxDepth);
        }
    }
}