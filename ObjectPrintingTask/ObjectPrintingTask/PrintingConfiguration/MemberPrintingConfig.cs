using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class MemberPrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly string memberName;
        private readonly PrintingConfig<TOwner> parentConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, string memberName)
        {
            this.parentConfig = parentConfig;
            this.memberName = memberName;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario)
        {
            parentConfig.AddSerializingScenario(memberName, scenario);
            return parentConfig;
        }
    }
}