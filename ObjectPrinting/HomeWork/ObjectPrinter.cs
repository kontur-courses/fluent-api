namespace ObjectPrinting.HomeWork
{
    public class ObjectPrinter
    {
        public static PrintingConfig<TOwner> For<TOwner>()
        {
            return new PrintingConfig<TOwner>();
        }
    }
}