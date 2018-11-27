namespace ObjectPrinting
{
    public interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        string NameMember { get; set; }
    }
}