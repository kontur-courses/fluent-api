using System;

namespace ObjectPrinting.Solved.PrintingConfiguration
{
    public class MemberPrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly string memberName;

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, string memberName)
        {
            ParentConfig = printingConfig;
            this.memberName = memberName;
        }

        public PrintingConfig<TOwner> ParentConfig { get; }


        public PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario)
        {
            ParentConfig.AddSerializingScenario(memberName, scenario);
            return ParentConfig;
        }
    }
}