using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this MemberPrintingConfig<TOwner, TPropType> memberPrintingConfig,
            CultureInfo culture
        )
            where TPropType : IFormattable
        {
            string Func(TPropType obj) => obj.ToString(null, culture);
            return memberPrintingConfig.Using(Func);
        }
    }
}