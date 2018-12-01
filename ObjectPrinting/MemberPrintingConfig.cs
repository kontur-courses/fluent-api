using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TPropType> : IMemberPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string memberName;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, string memberName)
        {
            this.printingConfig = printingConfig;
            this.memberName = memberName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberName == null)
                ((IPrintingConfig<TOwner>)printingConfig).TypesWithSpecificPrint.Add(typeof(TPropType), print);
            else
                ((IPrintingConfig<TOwner>)printingConfig).NamesWithSpecificPrint.Add(memberName, print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig<TOwner>)printingConfig).TypesWithCulture.Add(typeof(TPropType), culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        string IMemberPrintingConfig<TOwner, TPropType>.MemberNameToTrimValue => memberName;
    }

    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string MemberNameToTrimValue { get; }
    }
}