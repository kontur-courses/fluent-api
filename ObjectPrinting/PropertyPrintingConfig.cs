using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        internal readonly PrintingConfig<TOwner> PrintingConfig;
        internal readonly PropertyInfo PropertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo = null)
        {
            PrintingConfig = printingConfig;
            PropertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
        {
            if (PropertyInfo is null)
                PrintingConfig.CustomTypeSerializers[typeof(TPropType)] = printingMethod;
            else
                PrintingConfig.CustomPropertySerializers[PropertyInfo] = printingMethod;
            
            return PrintingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => PrintingConfig;
    }
    
    interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}