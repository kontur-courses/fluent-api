using System;
using System.Reflection;

namespace ObjectPrinting.Contexts
{
    public class PropertyPrintingContext<TOwner, TPropType>
    {
        private readonly PrintingConfig config;
        private readonly MemberInfo propertyInfo;

        public PropertyPrintingContext(PrintingConfig config, MemberInfo propertyInfo)
        {
            this.config = config;
            this.propertyInfo = propertyInfo;
        }

        public ConfigPrintingContext<TOwner> Using(Func<TPropType, string> serializeProperty) =>
            new ConfigPrintingContext<TOwner>(config with
            {
                MemberPrinting =
                config.MemberPrinting.SetItem(propertyInfo, value => serializeProperty((TPropType)value))
            });
    }
}