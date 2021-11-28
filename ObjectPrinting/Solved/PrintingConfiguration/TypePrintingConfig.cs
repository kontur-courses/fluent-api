using System;

namespace ObjectPrinting.Solved.PrintingConfiguration
{
    public class TypePrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly Type type;

        public TypePrintingConfig(PrintingConfig<TOwner> parentConfig, Type type)
        {
            ParentConfig = parentConfig;
            this.type = type;
        }

        public PrintingConfig<TOwner> ParentConfig { get; }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario)
        {
            ParentConfig.AddSerializingScenario(type, scenario);
            return ParentConfig;
        }
    }
}