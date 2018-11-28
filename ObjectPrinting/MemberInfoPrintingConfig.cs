using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class MemberInfoPrintingConfig<TOwner, TPropType> : IMemberInfoPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public MemberInfoPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public MemberInfoPrintingConfig(PrintingConfig<TOwner> printingConfig, string memberInfoName)
        {
            this.printingConfig = printingConfig;
            MemberInfoName = memberInfoName;
        }

        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            printingConfig.AddTypeSerializationCulture(typeof(TPropType), cultureInfo);
            return printingConfig;
        }
        
        public string MemberInfoName { get; }

        PrintingConfig<TOwner> IMemberInfoPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializeFunc)
        {
            printingConfig.AddTypeSerialization(typeof(TPropType), serializeFunc);
            return printingConfig;
        }
    }

    public interface IMemberInfoPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}