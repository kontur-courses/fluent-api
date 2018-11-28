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
            if (obj is int i)
                return i.ToString(cultureInfo);
            if (obj is double d)
                return d.ToString(cultureInfo);
            if (obj is float f)
                return f.ToString(cultureInfo);

            throw new ArgumentException("Expected argument to be", nameof(obj));
        }
    }
}
