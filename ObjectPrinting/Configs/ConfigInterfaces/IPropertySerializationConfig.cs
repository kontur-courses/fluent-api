namespace ObjectPrinting.Configs.ConfigInterfaces
{
    public interface IPropertySerializationConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}