using System;
using System.Linq.Expressions;
using ObjectPrinting.Configs.ConfigInterfaces;

namespace ObjectPrinting.Configs
{
    public class PropertySerializationConfig<TOwner, TPropType>
        : IPropertySerializationConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        public PrintingConfig<TOwner> Using(Expression<Func<TPropType, string>> serializationFunc)
        {
            return parentConfig;
        }
        
        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        PrintingConfig<TOwner> IPropertySerializationConfig<TOwner>.ParentConfig => parentConfig;
    }
}