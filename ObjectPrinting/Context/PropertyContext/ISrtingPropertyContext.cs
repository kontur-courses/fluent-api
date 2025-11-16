namespace ObjectPrinting.Context;

public interface IStringPropertyPrinterConfigContext<TOwner> : IPropertyPrinterConfigContext<TOwner, string>
{
    IStringPropertyPrinterConfigContext<TOwner> TrimToLength(int maxLength);
}