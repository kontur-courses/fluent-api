using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : PrintingConfig<TOwner>
    {
        public TypePrintingConfig(PrintingConfig<TOwner> config) : base(config)
        {
        }

        public TypePrintingConfig<TOwner, TPropType> WithCulture<T>(CultureInfo cultureInfo) where T : IFormattable
        {
            typesCulture.Add(typeof(T), cultureInfo);

            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Using(Func<TPropType, string> printType)
        {
            typesSerialization.TryAdd(typeof(TPropType), type => printType((TPropType)type));

            return this;
        }
    }
}