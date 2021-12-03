using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner, TMemberType>
    {
        public PrintingConfig<TOwner> ParentConfig => printingConfig;
        
        private readonly PrintingConfig<TOwner> printingConfig;
        
        private readonly MemberInfo member;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            this.member = typeof(TMemberType);
        }
        
        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member)
        {
            if (member.GetReturnType() is TMemberType)
                throw new ArgumentException($"{member.Name} is not {nameof(TMemberType)}");
            this.printingConfig = printingConfig;
            this.member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> serializer)
        {
            return printingConfig.SpecializeSerialization(member, obj => serializer((TMemberType) obj));
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            Func<object, string> culturedConverter = x => Convert.ToString(x, culture);
            return printingConfig.SpecializeSerialization(member, culturedConverter);
        }
    }

}