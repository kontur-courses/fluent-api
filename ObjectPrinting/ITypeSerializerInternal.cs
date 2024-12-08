namespace ObjectPrinting;

internal interface ITypeSerializerInternal<TOwner>
{
    public PrintingConfig<TOwner> Config { get; }
}