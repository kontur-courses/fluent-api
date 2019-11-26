namespace ObjectPrinting
{
    public interface IPropertySerializationConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}