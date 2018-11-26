using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty> : IPropertyPrintingConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config, string propertyName = null)
        {
            this.config = config;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializer)
        {
            if (propertyName == null)
                config.AddAlternativeTypeSerializer(serializer);
            else
                config.AddAlternativePropertySerializer(serializer, propertyName);
            return config;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TProperty>.PrintingConfig => config;
        string IPropertyPrintingConfig<TOwner, TProperty>.PropertyName => propertyName;
    }
}