using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class TypePrintingConfig<TOwner, TMemberType>
        : IChildPrintingConfig<TOwner, TMemberType>
    {
        private readonly Type type;
        private readonly PrintingConfig<TOwner> parentConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> parentConfig, Type type)
        {
            this.parentConfig = parentConfig;
            this.type = type;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario)
        {
            parentConfig.AddSerializingScenario(type, scenario);
            return parentConfig;
        }
    }
}