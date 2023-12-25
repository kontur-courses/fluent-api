namespace ObjectPrinting
{
    public interface IInnerPrintingConfig<TOwner, TTypeOrPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}