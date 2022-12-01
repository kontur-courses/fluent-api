using System;

namespace ObjectPrinting.PrintingConfiguration
{
    public class TypePrintingConfig<TOwner, TType>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            PrintingConfig.ExcludeType(typeof(TType));
            return PrintingConfig;
        }

        public PrintingConfig<TOwner> ChangeSerialization(Func<TType, string> func)
        {
            var type = typeof(TType);
            PrintingConfig.AddSerializationTypeRule(type, func);
            return PrintingConfig;
        }
    }
}