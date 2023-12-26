using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class UsingExtensions
    {
        public static IWrap<TOwner> Using<TOwner, TSerialization>(
            this IUsing<TOwner, TSerialization> @using,
            CultureInfo culture)
            where TSerialization : IFormattable
        {
            return @using.Using(
                p => p.ToString(null, culture));
        }

        public static IWrap<TOwner> Trim<TOwner>(
            this IUsing<TOwner, string> @using,
            int length)

        {
            return @using.Using(value => StringHelper.Trim(value, length));
        }
    }
}