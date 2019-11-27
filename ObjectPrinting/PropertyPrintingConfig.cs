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
            var config = new PrintingConfig<TOwner>(parentConfig);
            if (propertyInfo == null)
                (config as IPrintingConfig).CustomTypesPrints[typeof(TProperty)] = value => serializationFunc((TProperty)value);
            else
                (config as IPrintingConfig).CustomPropertiesPrints[propertyInfo] = value => serializationFunc((TProperty)value);
            return config;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertyPrintingConfig<TOwner>.PropertyInfo => propertyInfo;
    }
}
