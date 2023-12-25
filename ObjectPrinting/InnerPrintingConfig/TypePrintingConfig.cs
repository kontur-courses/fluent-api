using System;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.InnerPrintingConfig
{
    public class TypePrintingConfig<TOwner, TType> : IInnerPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> print)
        {
            printingConfig.TypeSerializers[typeof(TType)] = obj => print((TType)obj);
            return printingConfig;
        }
        
        public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
        {
            var isSerialized = printingConfig.TypeSerializers.TryGetValue(typeof(TType), out var prevSerializer);
            printingConfig.TypeSerializers[typeof(TType)] = isSerialized 
                ? obj => prevSerializer(obj).Truncate(maxLen) 
                : obj => obj.ToString().Truncate(maxLen);
            return printingConfig;
        }
    }
}