namespace ObjectPrinting
{
    interface ISerializingConfig<TOwner>
    {
        SerializingConfigContext Context { get; }
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}