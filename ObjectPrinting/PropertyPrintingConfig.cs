using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TType>
{
    internal PrintingConfig<TOwner> ParentConfig => _printingConfig;
    internal readonly MemberInfo? Member;
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
        return _printingConfig;
    }
}
