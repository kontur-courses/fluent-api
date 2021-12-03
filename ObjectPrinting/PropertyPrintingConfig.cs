using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private PrintingConfig<TOwner> parentConfig;
        public PropertyInfo PropertyInfo { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo = null)
        {
            this.parentConfig = parentConfig;
            PropertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (PropertyInfo == null)
            {
                parentConfig.TypeToPrinting[typeof(TPropType)] = print;
            }
            else
            {
                parentConfig.PropertyToPrinting[PropertyInfo] = print;
            }

            return parentConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => parentConfig;
    }
}