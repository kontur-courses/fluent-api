using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            var typeProperty = typeof(TPropType);
            if (printingConfig.AlternativeSerializationByType.ContainsKey(typeProperty))
                printingConfig.AlternativeSerializationByType[typeProperty] = serializationMethod;
            else printingConfig.AlternativeSerializationByType.Add(typeProperty, serializationMethod);
            return printingConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }
}