using System;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.InnerPrintingConfigs
{
    public class TypePrintingConfig<TOwner, TType> : PrintingConfig<TOwner>, IInnerPrintingConfig<TOwner, TType>
    {
        internal TypePrintingConfig(PrintingConfig<TOwner> parent) : base(parent)
        { }

        public PrintingConfig<TOwner> Using(Func<TType, string> print)
        {
            TypeSerializers[typeof(TType)] = obj => print((TType)obj);
            return this;
        }
        
        public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
        {
            var isSerialized = TypeSerializers.TryGetValue(typeof(TType), out var prevSerializer);
            TypeSerializers[typeof(TType)] = isSerialized 
                ? obj => prevSerializer(obj).Truncate(maxLen) 
                : obj => obj.ToString().Truncate(maxLen);
            return this;
        }
    }
}