using System;
using System.Linq.Expressions;
using ObjectPrinting.Configs.Interfaces;

namespace ObjectPrinting.Configs;

public class PropertyPrintingConfig<TOwner, TPropType> : IChildPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    public readonly string PropertyPath;

    PrintingConfig<TOwner> IChildPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

    internal PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, Expression<Func<TOwner, TPropType>> selector)
    {
        printingConfig = parentConfig ?? throw new ArgumentNullException(nameof(parentConfig));
        PropertyPath = BuildMemberPath(selector);
    }

    public PrintingConfig<TOwner> Use(Func<TPropType, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        printingConfig.Settings.PropertySerializers[PropertyPath] = obj => serializer((TPropType)obj!);
        return printingConfig;
    }

    public PrintingConfig<TOwner> Exclude()
    {
        printingConfig.Settings.ExcludedProperties.Add(PropertyPath);
        return printingConfig;
    }

    private static string BuildMemberPath(Expression<Func<TOwner, TPropType>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var expression = selector.Body;
        var parts = new System.Collections.Generic.List<string>();
        while (expression is MemberExpression m)
        {
            parts.Add(m.Member.Name);
            expression = m.Expression;
        }

        parts.Reverse();
        var ownerName = typeof(TOwner).Name;
        return ownerName + "." + string.Join('.', parts);
    }
}