using System;
using System.Reflection;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class MemberPrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly MemberInfo member;
        private readonly Printer<TOwner> printer;

        public MemberPrintingConfig(Printer<TOwner> printer, MemberInfo member)
        {
            this.printer = printer;
            this.member = member;
        }

        public Printer<TOwner> Using(Func<TMemberType, string> scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException("Alternate scenario can not be null");

            printer.AddSerializingScenario(member, scenario);
            return printer;
        }
    }
}