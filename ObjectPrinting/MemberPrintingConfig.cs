using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner, TMemberType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo member;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            this.member = typeof(TMemberType);
        }
        
        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member)
        {
            this.printingConfig = printingConfig;
            this.member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> serializer)
        {
            return printingConfig.SpecializeSerialization(member, serializer);
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            Func<object, string> func = x => Convert.ToString(x, culture);
            return printingConfig.SpecializeSerialization(member, func);
        }

        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemberType>.ParentConfig => printingConfig;
    }

    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}