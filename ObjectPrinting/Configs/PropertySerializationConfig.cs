using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Configs.ConfigInterfaces;

namespace ObjectPrinting.Configs
{
    public class PropertySerializationConfig<TOwner, TPropType>
        : IPropertySerializationConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;
        
        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
            propertyInfo = null;
        }
        
        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationFunc)
        {
            var casted = (IPrintingConfig<TOwner>) parentConfig;
            if (propertyInfo != null) return casted.AddPropertySerialization(propertyInfo, o => serializationFunc((TPropType)o));
            return casted.AddTypeSerialization(typeof(TPropType), o => serializationFunc((TPropType)o));
        }

        PrintingConfig<TOwner> IPropertySerializationConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertySerializationConfig<TOwner>.PropertyInfo => propertyInfo;

    }
}