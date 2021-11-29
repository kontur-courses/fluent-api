using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMember> : IMemberPrintingConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly MemberInfo propertyInfo;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMember, string> serializationRule)
        {
            return new PrintingConfig<TOwner>(parentConfig, propertyInfo, serializationRule);
        }
    }
}
