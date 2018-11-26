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
            var typeMember = typeof(TPropType);
            if (printingConfig.AlternativeSerializationByType.ContainsKey(typeMember))
                printingConfig.AlternativeSerializationByType[typeMember] = serializationMethod;
            else printingConfig.AlternativeSerializationByType.Add(typeMember, serializationMethod);
            return printingConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }
}