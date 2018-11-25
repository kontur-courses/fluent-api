using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> ownerConfig;
        private readonly PropertyInfo property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> ownerConfig, PropertyInfo property = null)
        {
            this.ownerConfig = ownerConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
        {
            if (property != null)
            {
                ownerConfig.PropertiesSerializers[property] = printingMethod;
            }
            else
            {
                ownerConfig.TypesSerializers[typeof(TPropType)] = printingMethod;
            }

            return ownerConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.PrintingConfig => ownerConfig;

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property => property;
    }
}