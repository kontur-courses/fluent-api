using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, T> : IChildPrintingConfig<TOwner, T>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<T, string> serialize)
        {
            parentConfig.SetTypeTransformationRule(serialize, TypeTransformations.Serialization);
            return parentConfig;
        }

        PrintingConfig<TOwner> IChildPrintingConfig<TOwner, T>.ParentConfig => parentConfig;
    }
}