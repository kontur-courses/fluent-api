namespace ObjectPrinting
{
    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}