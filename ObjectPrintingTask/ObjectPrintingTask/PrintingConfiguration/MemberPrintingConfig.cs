using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class MemberPrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly string memberFullName;
        private readonly PrintingConfig<TOwner> parentConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, string memberFullName)
        {
            this.parentConfig = parentConfig;
            this.memberFullName = memberFullName;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario)
        {
            parentConfig.AddSerializingScenario(memberFullName, scenario);
            return parentConfig;
        }
    }
}