namespace ObjectPrinting
{
    public interface ITypePrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }
}