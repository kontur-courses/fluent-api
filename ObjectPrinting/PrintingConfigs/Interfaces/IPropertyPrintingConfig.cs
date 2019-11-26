using ObjectPrinting.Serializer;

namespace ObjectPrinting.PrintingConfigs
{
    internal interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
        SerializationFilter Filter { get; }
    }
}