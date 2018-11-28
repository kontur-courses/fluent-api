using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string nameMember;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            nameMember = null;
        }

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig, string memberName)
        {
            this.printingConfig = printingConfig;
            nameMember = memberName;
        }

        internal string GetNameMember()
        {
            return nameMember;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            var typeMember = typeof(TPropType);
            if (nameMember == null)
                printingConfig.AddAlternativeSerialization(typeMember, serializationMethod);
            else printingConfig.AddAlternativeSerialization(nameMember, serializationMethod);
            return printingConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

    }
}