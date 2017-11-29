using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp> : IChildPrintingConfig<TOwner, TProp>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> serialize)
        {
            parentConfig.SetPropertyTransformationRule(propertyInfo, serialize, PropertyTransformations.Serialization);

            return parentConfig;
        }

        PrintingConfig<TOwner> IChildPrintingConfig<TOwner, TProp>.ParentConfig => parentConfig;
    }
}