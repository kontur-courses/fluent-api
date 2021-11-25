using System;
using System.Reflection;

namespace ObjectPrinting.Contexts
{
    public class PropertyPrintingContext<TOwner>
    {
        private readonly PrintingConfig config;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingContext(PrintingConfig config, PropertyInfo propertyInfo)
        {
            this.config = config;
            this.propertyInfo = propertyInfo;
        }

        public ConfigPrintingContext<TOwner> Using(Func<PropertyInfo, string> serializeProperty)
        {
            return new ConfigPrintingContext<TOwner>(config with
            {
                PropertyPrinting = config.PropertyPrinting.SetItem(propertyInfo, serializeProperty)
            });
        }
    }
}