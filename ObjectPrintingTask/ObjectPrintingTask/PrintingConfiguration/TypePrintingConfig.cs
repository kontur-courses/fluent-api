using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class TypePrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly Type type;
        private readonly Printer<TOwner> printer;

        public TypePrintingConfig(Printer<TOwner> printer, Type type)
        {
            this.printer = printer;
            this.type = type;
        }

        public Printer<TOwner> Using(Func<TMemberType, string> scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException("Alternate scenario can not be null");

            printer.AddSerializingScenario(type, scenario);
            return printer;
        }
    }
}