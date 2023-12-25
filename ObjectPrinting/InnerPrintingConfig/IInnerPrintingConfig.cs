namespace ObjectPrinting.InnerPrintingConfig
{
    public interface IInnerPrintingConfig<TOwner, TTypeOrPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}