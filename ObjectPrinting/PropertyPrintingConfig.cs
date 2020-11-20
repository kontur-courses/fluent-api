using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;
        private readonly FieldInfo fieldInfo;


        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            PropertyInfo propertyInfo = null, FieldInfo fieldInfo = null)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
            this.fieldInfo = fieldInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var config = (IPrintingConfig<TOwner>)printingConfig;
            if (propertyInfo != null)
                config.PropertySerialization[propertyInfo] = print;
            else if (fieldInfo != null)
                config.FieldSerialization[fieldInfo] = print;
            else
                config.TypeSerialization[typeof(TPropType)] = print;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.PropertyInfo => propertyInfo;
        FieldInfo IPropertyPrintingConfig<TOwner, TPropType>.FieldInfo => fieldInfo;
    }
}
