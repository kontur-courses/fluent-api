using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property = null)
        {
            this.printingConfig = printingConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
        {
            if (property != null)
            {
                printingConfig.PropertiesSerializers[property] = printingMethod;
            }
            else
            {
                printingConfig.TypesSerializers[typeof(TPropType)] = printingMethod;
            }

            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.PrintingConfig => printingConfig;

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property => property;
    }
}