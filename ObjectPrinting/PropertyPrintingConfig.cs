using System;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty> : IMemberPrintingConfig<TOwner, TProperty>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            ParentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializationRule)
        {
            return new PrintingConfig<TOwner>(ParentConfig, propertyInfo, serializationRule);
        }
    }
}
