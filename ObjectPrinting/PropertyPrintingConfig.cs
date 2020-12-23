using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public PrintingConfig<TOwner> ParentPrintingConfig { get; set; }

        public PropertyInfo Property { get; set; }

        public PropertyPrintingConfig(PropertyInfo property, PrintingConfig<TOwner> parentPrintingConfig)
        {
            Property = property;
            ParentPrintingConfig = parentPrintingConfig;
        }

        public PrintingConfig<TOwner> SetSerializer(Func<TProperty, string> serializer)
        {
            var newConfig = new PrintingConfig<TOwner>(ParentPrintingConfig);
            newConfig.AltSerializerForProperty = ParentPrintingConfig
                .AltSerializerForProperty.AddOrUpdate(Property, serializer);
            return newConfig;
        }
    }
}
