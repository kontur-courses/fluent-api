using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string Serialize<TOwner>(this TOwner owner,
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config = null)
        {
            return config != null
                ? config(ObjectPrinter.For<TOwner>()).PrintToString(owner)
                : ObjectPrinter.For<TOwner>().PrintToString(owner);
        }

        public static string ToStringWithCulture(this object obj, CultureInfo cultureInfo)
        {
            if (obj is IFormattable formattable)
                return formattable.ToString(null, cultureInfo);

            throw new ArgumentException("Expected argument to be", nameof(obj));
        }
    }
}
