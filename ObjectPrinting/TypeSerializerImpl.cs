using System;

namespace ObjectPrinting;

internal class TypeSerializerImpl<TParam, TOwner> : ITypeSerializer<TParam, TOwner>
{
    public PrintingConfig<TOwner> Config { get; }

    internal TypeSerializerImpl(PrintingConfig<TOwner> config)
    {
        Config = config;
    }
        
    public PrintingConfig<TOwner> Use(Func<TParam, string> converter)
    {
        if (converter == null)
            throw new ArgumentNullException($"{nameof(converter)} cannot be null");
        Config.AddTypeConverter(typeof(TParam), converter);
        return Config;
    }
}