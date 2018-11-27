using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ObjectPrinting
{
    public static class SerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Cut<TOwner>(this SerializingConfig<TOwner, string> config, int strLen)
        {
            var cfg = ((ISerializingConfig<TOwner>) config);
            cfg.Context.TypeSerializers.Add(typeof(string), (str) => ((string) str).Cut(strLen));
            return cfg.ParentConfig;
        }
    }
}