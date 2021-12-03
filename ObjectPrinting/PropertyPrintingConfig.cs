using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public PropertyInfo PropertyInfo { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo = null)
        {
            ParentConfig = parentConfig;
            PropertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (PropertyInfo == null)
            {
                ParentConfig.TypeToPrinting[typeof(TPropType)] = print;
            }
            else
            {
                ParentConfig.PropertyToPrinting[PropertyInfo] = print;
            }

            return ParentConfig;
        }
    }
}