using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, int> pc, CultureInfo ci)
        {
            return ((ITypePrintingConfig<TOwner>)pc).PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this TypePrintingConfig<TOwner, string> pc, int length)
        {
            return ((ITypePrintingConfig<TOwner>)pc).PrintingConfig;
        }
    }
}