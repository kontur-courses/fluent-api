using System;
using ObjectPrinting.Context;

namespace ObjectPrinting;

public class StringPropertyContext<TOwner> : IStringPropertyPrinterConfigContext<TOwner>
{
    private readonly PrintingConfig<TOwner> parent;
    private readonly string propertyName;

    internal StringPropertyContext(PrintingConfig<TOwner> parent, string propertyName)
    {
        this.parent = parent;
        this.propertyName = propertyName;
    }

    public IStringPropertyPrinterConfigContext<TOwner> Set(Func<string, string> serializer)
    {
        parent.SetPropertySerializer(propertyName, serializer);
        return this;
    }

    public IStringPropertyPrinterConfigContext<TOwner> Exclude()
    {
        parent.ExcludeProperty(propertyName);
        return this;
    }

    public IStringPropertyPrinterConfigContext<TOwner> TrimToLength(int maxLength)
    {
        parent.TrimStringProperty(propertyName, maxLength);
        return this;
    }

    public PrintingConfig<TOwner> End()
    {
        return parent;
    }

    IPropertyPrinterConfigContext<TOwner, string> IPropertyPrinterConfigContext<TOwner, string>.SetSerializer(Func<string, string> serializer)
    {
        return Set(serializer);
    }

    IPropertyPrinterConfigContext<TOwner, string> IPropertyPrinterConfigContext<TOwner, string>.ExcludeProperty()
    {
        return Exclude();
    }
}