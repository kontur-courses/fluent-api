namespace ObjectPrinting.Solved
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropName { get; }
    }
}