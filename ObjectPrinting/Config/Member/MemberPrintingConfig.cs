using System;
using System.Reflection;

namespace ObjectPrinting.Config.Member
{
    public class MemberPrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public readonly MemberInfo MemberToChange;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberToChange)
        {
            ParentConfig = printingConfig;
            MemberToChange = memberToChange;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            ParentConfig.OverrideMember(MemberToChange, obj => print((TPropType) obj));

            return ParentConfig;
        }
    }
}