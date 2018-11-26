namespace ObjectPrinting.SerializingConfig
{
    public interface ISerializingConfig<TOwner, TPropertyType>
    {
        PrintingConfig<TOwner> SerializingConfig { get; }
    }
}