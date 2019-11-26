using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializationFunc)
        {
            if (propertyInfo == null)
                (parentConfig as IPrintingConfig).CustomTypesPrints[typeof(TProperty)] = value => serializationFunc((TProperty)value);
            else
                (parentConfig as IPrintingConfig).CustomPropertiesPrints[propertyInfo.Name] = value => serializationFunc((TProperty)value);
            return parentConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertyPrintingConfig<TOwner>.PropertyInfo => propertyInfo;
    }
}
