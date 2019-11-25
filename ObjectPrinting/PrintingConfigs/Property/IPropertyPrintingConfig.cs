namespace ObjectPrinting
{
    internal interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
        SerializationFilter Filter { get; }
    }
}