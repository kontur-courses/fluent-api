using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp> : IChildPrintingConfig<TOwner, TProp>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, string propertyName)
        {
            this.parentConfig = parentConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> serialize)
        {
            parentConfig.SetPropertyTransformationRule(propertyName, serialize);
            return parentConfig;
        }

        PrintingConfig<TOwner> IChildPrintingConfig<TOwner, TProp>.ParentConfig => parentConfig;
    }
}