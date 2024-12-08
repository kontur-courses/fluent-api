using System;
using System.Reflection;

namespace ObjectPrinting;

internal class PropertySerializerImpl<TOwner, TProperty> : IPropertySerializer<TOwner, TProperty>
{
    public PrintingConfig<TOwner> Config { get; }
    private readonly MemberInfo _memberInfo;

    internal PropertySerializerImpl(PrintingConfig<TOwner> config, MemberInfo memberInfo)
    {
        Config = config;
        _memberInfo = memberInfo;
    }
    
    public PrintingConfig<TOwner> Use(Func<TProperty, string> converter)
    {
        Config.AddPropertyConverter(converter, _memberInfo);
        return Config;
    }
}