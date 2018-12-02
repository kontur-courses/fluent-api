using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this TypePrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo culture) where TPropType : IFormattable
        {
            var parentConfig = ((ITypePrintingConfig<TOwner, TPropType>) propConfig).ParentConfig;
            parentConfig.ChangeCultureInfoForType(typeof(TPropType), culture);
            return parentConfig;
        }
    }
}