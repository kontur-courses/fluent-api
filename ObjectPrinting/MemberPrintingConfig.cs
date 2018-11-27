using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TPropType> : IMemberPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> ownerConfig;
        private readonly MemberInfo memberInfo;

        public MemberPrintingConfig(PrintingConfig<TOwner> ownerConfig, MemberInfo memberInfo = null)
        {
            this.ownerConfig = ownerConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
        {
            if (memberInfo != null)
            {
                ownerConfig.MembersSerializers[memberInfo] = printingMethod;
            }
            else
            {
                ownerConfig.TypesSerializers[typeof(TPropType)] = printingMethod;
            }

            return ownerConfig;
        }

        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TPropType>.PrintingConfig => ownerConfig;

        MemberInfo IMemberPrintingConfig<TOwner, TPropType>.MemberInfo => memberInfo;
    }
}