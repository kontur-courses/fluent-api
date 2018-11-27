using System.Globalization;
using System.Runtime.CompilerServices;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> pc, CultureInfo ci) 
        {
            pc.PrintingConfig.SetNumberCulture<int>(ci);
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<double>(ci);
            return ((ITypePrintingConfig<TOwner>)pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> pc, CultureInfo ci)
        {
            pc.PrintingConfig.SetNumberCulture<float>(ci);
            return ((ITypePrintingConfig<TOwner>)pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this TypePrintingConfig<TOwner, string> pc, int length)
        {
            return ((ITypePrintingConfig<TOwner>) pc).PrintingConfig;
        }
    }
}