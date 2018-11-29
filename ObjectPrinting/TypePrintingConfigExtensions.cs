using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> name,
            CultureInfo cultureInfo)
        {
            return name.Using(arg => arg.ToString(cultureInfo));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> name,
            CultureInfo cultureInfo)
        {
            return name.Using(arg => arg.ToString(cultureInfo));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> name,
            CultureInfo cultureInfo)
        {
            return name.Using(arg => arg.ToString(cultureInfo));
        }
    }
}