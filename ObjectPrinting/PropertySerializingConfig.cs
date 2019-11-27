using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;
        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.parentConfig = printingConfig;
        }
        
        public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            if (propertyInfo == null)
            {
                (parentConfig as IPrintingConfig).typeSerialisation[typeof(TPropType)] = serializationMethod;
                return parentConfig;
            }
            (parentConfig as IPrintingConfig).propertySerialisation[propertyInfo] = serializationMethod;
            return parentConfig;
        }
    }
}