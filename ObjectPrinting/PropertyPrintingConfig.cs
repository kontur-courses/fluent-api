using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly string propertyFullName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, string propertyFullName)
        {
            this.parentConfig = parentConfig;
            this.propertyFullName = propertyFullName;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializationFunc)
        {
            var config = new PrintingConfig<TOwner>(parentConfig);
            if (propertyFullName == null)
                (config as IPrintingConfig).CustomTypesPrints[typeof(TProperty)] = value => serializationFunc((TProperty)value);
            else
                (config as IPrintingConfig).CustomPropertiesPrints[propertyFullName] = value => serializationFunc((TProperty)value);
            return config;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
        string IPropertyPrintingConfig<TOwner>.PropertyFullName => propertyFullName;
    }
}
