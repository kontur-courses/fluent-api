using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo = null)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializationFunc)
        {
            return propertyInfo != null ? 
                (parentConfig as IPrintingConfig<TOwner>).AddCustomPrintForProperty(serializationFunc, propertyInfo) 
                : (parentConfig as IPrintingConfig<TOwner>).AddCustomPrintForType(serializationFunc);
        }
    }
}
