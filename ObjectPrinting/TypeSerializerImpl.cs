using System;

namespace ObjectPrinting;

internal class TypeSerializerImpl<TParam, TOwner> : ITypeSerializer<TParam, TOwner>, ITypeSerializerInternal<TOwner>
{
    public PrintingConfig<TOwner> Config { get; }

    internal TypeSerializerImpl(PrintingConfig<TOwner> config)
    {
        Config = config;
    }
        
    public PrintingConfig<TOwner> Use(Func<TParam, string> converter)
    {
        Config.AddConverter(typeof(TParam), converter);
        return Config;
    }
}