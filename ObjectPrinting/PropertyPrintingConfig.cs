using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty> : IPropertyPrintingConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> parentPrintingConfig;
        private readonly PropertyInfo property;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TProperty>.ParentPrintingConfig => parentPrintingConfig;

        public PropertyPrintingConfig(PropertyInfo property, PrintingConfig<TOwner> parentPrintingConfig)
        {
            this.property = property;
            this.parentPrintingConfig = parentPrintingConfig;
        }

        public PrintingConfig<TOwner> SetSerializer(Func<TProperty, string> serializer)
        {
            var newConfig = new PrintingConfig<TOwner>(parentPrintingConfig);
            ((IPrintingConfig)newConfig).State.AltSerializerForProperty = ((IPrintingConfig)parentPrintingConfig).
                State.AltSerializerForProperty.AddOrUpdate(property, serializer);
            return newConfig;
        }
    }

    public interface IPropertyPrintingConfig<TOwner, TProperty>
    {
        PrintingConfig<TOwner> ParentPrintingConfig { get; }
    }
}
