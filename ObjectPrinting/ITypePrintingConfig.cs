namespace ObjectPrinting
{
    public interface ITypePrintingConfig<TOwner>
    {
        IPrintingConfig<TOwner> PrintingConfig { get; }
        string NameMember { get; set; }
    }
}