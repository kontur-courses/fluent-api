using System;
using System.Linq.Expressions;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Configs;

public class PropertyPrintingConfig<TOwner, TProp>(
    PrintingConfig<TOwner> parent,
    Expression<Func<TOwner, TProp>> selector)
{
    private readonly string propertyName = PrintingConfigExtensions.GetPropertyName(selector);

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        parent.PropertySerializers[propertyName] =
            x => serializer((TProp)x);

        return parent;
    }

    public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
    {
        if (typeof(TProp) != typeof(string))
            throw new InvalidOperationException(
                "TrimmedToLength is only allowed for string properties");

        parent.StringTrimmingRules[propertyName] = maxLen;
        return parent;
    }
}