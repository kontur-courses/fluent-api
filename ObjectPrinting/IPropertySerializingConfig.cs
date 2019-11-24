namespace ObjectPrinting
{
    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string Serialize(object property);
    }
}
