namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        SerializationSettings SerializationSettings { get; }
    }
}
