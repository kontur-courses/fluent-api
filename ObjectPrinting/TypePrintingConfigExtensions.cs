using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<int>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, long> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<long>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, short> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<short>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, byte> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<byte>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<double>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<float>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, decimal> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<decimal>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }
    }
}