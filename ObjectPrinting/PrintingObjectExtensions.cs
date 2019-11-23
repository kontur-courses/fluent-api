namespace ObjectPrinting
{
    public static class PrintingObjectExtensions
    {
        public static PrintingConfig<TOwner> Printing<TOwner>(this TOwner obj)
        {
            return new PrintingConfig<TOwner>(obj);
        }
    }
}