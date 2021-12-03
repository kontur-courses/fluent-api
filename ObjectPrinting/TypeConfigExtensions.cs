using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypeConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this TypeConfig<TOwner, TPropType> memberPrintingConfig, CultureInfo culture) where TPropType : IFormattable
        {
            return memberPrintingConfig.Using(obj => obj.ToString(null, culture));
        }
    }
}