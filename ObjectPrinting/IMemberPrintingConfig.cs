namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TMemType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
    }
}