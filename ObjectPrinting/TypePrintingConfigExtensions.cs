using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, int>) pc).PrintingConfig.SetNumberCulture<int>(ci);
            return  ((ITypePrintingConfig<TOwner, int>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, long> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, long>) pc).PrintingConfig.SetNumberCulture<long>(ci);
            return ((ITypePrintingConfig<TOwner, long>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, short> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, short>) pc).PrintingConfig.SetNumberCulture<short>(ci);
            return  ((ITypePrintingConfig<TOwner, short>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, byte> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, byte>) pc).PrintingConfig.SetNumberCulture<byte>(ci);
            return  ((ITypePrintingConfig<TOwner, byte>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, double>) pc).PrintingConfig.SetNumberCulture<double>(ci);
            return  ((ITypePrintingConfig<TOwner, double>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, float>) pc).PrintingConfig.SetNumberCulture<float>(ci);
            return  ((ITypePrintingConfig<TOwner, float>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, decimal> pc, CultureInfo ci)
        {
            ((ITypePrintingConfig<TOwner, decimal>) pc).PrintingConfig.SetNumberCulture<decimal>(ci);
            return  ((ITypePrintingConfig<TOwner, decimal>) pc).PrintingConfig;
        }
    }
}