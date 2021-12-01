using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner>
    {
        private readonly PropertyInfo property;
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo property)
        {
            this.property = property;
            printingConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<PropertyInfo, string> serializer)
        {
            printingConfig.AddAlternativePropertySerializer(property, serializer);
            return printingConfig;
        }
    }
}