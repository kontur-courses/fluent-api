using System;
using System.Reflection;

namespace ObjectPrinting.Configuration;

public class PropertyPrintingConfig<TOwner, TType>
{
    internal PrintingConfig<TOwner> ParentConfig => _printingConfig;
    internal readonly MemberInfo? Member = null;
    private readonly PrintingConfig<TOwner> _printingConfig;

    internal PropertyPrintingConfig(PrintingConfig<TOwner> config) 
    {
        _printingConfig = config;
    }

    internal PropertyPrintingConfig(PrintingConfig<TOwner> config, MemberInfo member) 
        : this(config)
    {
        Member = member;
    }

    public PrintingConfig<TOwner> Using(Func<TType, string> serializer)
    {
        var rule = _printingConfig.GetOrCreateRule<TType>(Member);

        rule.Serializer = value => serializer((TType)value);
        return _printingConfig;
    }
}
