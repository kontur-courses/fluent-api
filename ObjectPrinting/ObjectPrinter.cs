namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>(int depth = 10)
        {
            return new PrintingConfig<T>(depth);
        }
    }
}