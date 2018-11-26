namespace ObjectPrinting
{
    interface ISerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> SerializingConfig { get; }
    }
}