using System.Globalization;

namespace ObjectPrinting
{
    public static class TypeSerializingContextExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializingContext<TOwner, int> context,
            CultureInfo culture)
        {
            var config = ((ITypeSerializingContext<TOwner>)context).Config;
            ((IPrintingConfig<TOwner>) config).CultureInfoSettings.Add(typeof(int), culture);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializingContext<TOwner, long> context,
            CultureInfo culture)
        {
            var config = ((ITypeSerializingContext<TOwner>)context).Config;
            ((IPrintingConfig<TOwner>)config).CultureInfoSettings.Add(typeof(long), culture);
            return config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializingContext<TOwner, double> context,
            CultureInfo culture)
        {
            var config = ((ITypeSerializingContext<TOwner>)context).Config;
            ((IPrintingConfig<TOwner>)config).CultureInfoSettings.Add(typeof(double), culture);
            return config;
        }
    }
}