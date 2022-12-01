using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public PrintingConfig<TOwner> ParentConfig => printingConfig;

        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Configurations configurations;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Configurations configurations, PropertyInfo propertyInfo = null)
        {
            this.printingConfig = printingConfig;
            this.configurations = configurations;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> print)
        {
            if (propertyInfo is null)
            {
                if (!configurations.SerializationOfTypes.TryAdd(typeof(TProperty), print))
                    configurations.SerializationOfTypes[typeof(TProperty)] = print;
            }
            else
            {
                if (!configurations.SerializationOfProperties.TryAdd(propertyInfo, print))
                    configurations.SerializationOfProperties[propertyInfo] = print;
            }
                
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            configurations.Cultures.Add(typeof(TProperty), culture);
            return printingConfig;
        }
    }
}
