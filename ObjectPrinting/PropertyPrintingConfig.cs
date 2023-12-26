using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : PrintingConfig<TOwner>
    {
        private readonly MemberInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config, MemberInfo propertyInfo) : base(config)
        {
            this.propertyInfo = propertyInfo;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Using(Func<TPropType, string> printProperty)
        {
            propertiesSerialization.TryAdd(propertyInfo, type => printProperty((TPropType)type));

            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> TrimToLength(int length)
        {
            if (length < 0)
                throw new ArgumentException("length can not be negative");

            propertiesMaxLength.Add(propertyInfo, length);

            return this;
        }
    }
}