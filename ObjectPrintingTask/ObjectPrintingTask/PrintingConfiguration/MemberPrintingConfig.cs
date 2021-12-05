using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class MemberPrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly string memberFullName;
        private readonly Printer<TOwner> printer;

        public MemberPrintingConfig(Printer<TOwner> printer, string memberFullName)
        {
            this.printer = printer;
            this.memberFullName = memberFullName;
        }

        public Printer<TOwner> Using(Func<TMemberType, string> scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException("Alternate scenario can not be null");

            printer.AddSerializingScenario(memberFullName, scenario);
            return printer;
        }
    }
}