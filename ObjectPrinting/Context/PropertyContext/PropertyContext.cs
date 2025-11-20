using System;
using ObjectPrinting.Context;

namespace ObjectPrinting;

public class PropertyContext<TOwner, T> : IPropertyPrinterConfigContext<TOwner, T>
{
    private readonly PrintingConfig<TOwner> parent;
    private readonly string propertyName;

    internal PropertyContext(PrintingConfig<TOwner> parent, string propertyName)
    {
        this.parent = parent;
        this.propertyName = propertyName;
    }

    public IPropertyPrinterConfigContext<TOwner, T> SetSerializer(Func<T, string> serializer)
    {
        parent.SetPropertySerializer(propertyName, serializer);
        return this;
    }

    public IPropertyPrinterConfigContext<TOwner, T> ExcludeProperty()
    {
        parent.ExcludeProperty(propertyName);
        return this;
    }

    public PrintingConfig<TOwner> End()
    {
        return parent;
    }
}