namespace ObjectPrinting
{
    public interface ITypeSerializingContext<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
    }
}