using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo currentCulture)
        {
            ((config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig).CustomTypesPrints.Add(typeof(int), value => currentCulture.ToString());
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int length)
        {
            var propertyInfo = (config as IPropertyPrintingConfig<TOwner>).PropertyInfo;
            var parentConfig = (config as IPropertyPrintingConfig<TOwner>).ParentConfig;

            if (propertyInfo == null)
                (parentConfig as IPrintingConfig).CustomTypesPrints[typeof(string)] = value => ((string)value).Substring(0, length);
            else
                (parentConfig as IPrintingConfig).CustomPropertysPrints[propertyInfo.Name] = value => ((string)value).Substring(0, length);
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> SetMaxNumberItems<TOwner>(this PropertyPrintingConfig<TOwner, IEnumerable> config, int maxNumberListItems)
        {
            var parentConfig = (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
            (parentConfig as IPrintingConfig).MaxNumberListItems = maxNumberListItems;
            return parentConfig;
        }
    }
}
