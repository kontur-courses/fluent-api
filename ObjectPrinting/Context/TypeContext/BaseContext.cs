using System;
using ObjectPrinting.Context;

namespace ObjectPrinting;

public class BaseContext<TOwner, T> : IPrintingConfigContext<TOwner, T>
{
    private readonly PrintingConfig<TOwner> parent;

    internal BaseContext(PrintingConfig<TOwner> parent)
    {
        this.parent = parent;
    }

    public IPrintingConfigContext<TOwner, T> SetSerializer(Func<T, string> serializer)
    {
        parent.SetTypeSerializer(serializer);
        return this;
    }

    public IPrintingConfigContext<TOwner, T> UsingCulture(IFormatProvider culture)
    {
        parent.SetNumericCulture<T>(culture);
        return this;
    }

    public IPrintingConfigContext<TOwner, T> ExcludeType()
    {
        parent.ExcludeType<T>();
        return this;
    }

    public PrintingConfig<TOwner> End()
    {
        return parent;
    }
}