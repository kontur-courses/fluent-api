using System;
using System.Runtime.Remoting.Messaging;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private string nameMember;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            nameMember = null;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            return nameMember == null ? UsingByType(serializationMethod) : NewMethod(serializationMethod);
        }

        private PrintingConfig<TOwner> UsingByType(Func<TPropType, string> serializationMethod)
        {
            var typeMember = typeof(TPropType);
            if (printingConfig.AlternativeSerializationByType.ContainsKey(typeMember))
                printingConfig.AlternativeSerializationByType[typeMember] = serializationMethod;
            else printingConfig.AlternativeSerializationByType.Add(typeMember, serializationMethod);
            return printingConfig;
        }

        private PrintingConfig<TOwner> NewMethod(Func<TPropType, string> serializationMethod)
        {
            if (printingConfig.AlternativeSerializationByName.ContainsKey(nameMember))
                printingConfig.AlternativeSerializationByName[nameMember] = serializationMethod;
            else printingConfig.AlternativeSerializationByName.Add(nameMember, serializationMethod);
            return printingConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

        string ITypePrintingConfig<TOwner>.NameMember
        {
            get => nameMember;
            set => nameMember = value;
        }
    }
}