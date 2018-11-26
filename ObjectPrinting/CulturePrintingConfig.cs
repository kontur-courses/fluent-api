namespace ObjectPrinting
{
    public class CulturePrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> For<T>()
        {
            return new PrintingConfig<TOwner>();
        }
    }
}