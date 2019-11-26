using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertyPrintingConfig<TOwner>.ConfiguredProperty => propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
            propertyInfo = null;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> SetFormat(Func<TPropType, string> serializationFunc)
        {
            var parent = parentConfig as IPrintingConfig<TOwner>;
            if (propertyInfo is null)
                parent.TypeToFormatter[typeof(TPropType)] = serializationFunc;
            else
                parent.PropertyToFormatter[propertyInfo] = serializationFunc;
            
            return parentConfig;
        }
    }
}
