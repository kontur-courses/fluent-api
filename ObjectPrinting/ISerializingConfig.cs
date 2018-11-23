namespace ObjectPrinting
{
    public interface ISerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> SerializingConfig { get; }
    }
}