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

        public PropertyPrintingConfig<TOwner, TPropType> Using(Func<dynamic, string> printProperty)
        {
            propertiesSerialization.TryAdd(propertyInfo, printProperty);

            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> TrimToLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);

            propertiesMaxLength.Add(propertyInfo, length);

            return this;
        }
    }
}