using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, int> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<int>(ci);
            return ((ITypePrintingConfig<TOwner, int>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, long> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<long>(ci);
            return ((ITypePrintingConfig<TOwner, long>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, short> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<short>(ci);
            return ((ITypePrintingConfig<TOwner, short>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, byte> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<byte>(ci);
            return ((ITypePrintingConfig<TOwner, byte>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, double> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<double>(ci);
            return ((ITypePrintingConfig<TOwner, double>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, float> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<float>(ci);
            return ((ITypePrintingConfig<TOwner, float>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this ITypePrintingConfig<TOwner, decimal> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<decimal>(ci);
            return ((ITypePrintingConfig<TOwner, decimal>) pc).PrintingConfig;
        }
    }
}