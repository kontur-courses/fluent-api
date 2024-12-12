using System;
using System.Reflection;

namespace ObjectPrinting;

internal class PropertySerializerImpl<TOwner, TProperty> : IPropertySerializer<TOwner, TProperty>
{
    public PrintingConfig<TOwner> Config { get; }
    public readonly MemberInfo MemberInfo;

    internal PropertySerializerImpl(PrintingConfig<TOwner> config, MemberInfo memberInfo)
    {
        Config = config;
        MemberInfo = memberInfo;
    }
    
    public PrintingConfig<TOwner> Use(Func<TProperty, string> converter)
    {
        Config.AddPropertyConverter(converter, MemberInfo);
        return Config;
    }
}