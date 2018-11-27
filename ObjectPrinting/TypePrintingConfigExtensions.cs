using System;
using System.Globalization;

namespace ObjectPrinting
{

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, int> typePrintingConfig, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, double> typePrintingConfig, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
        }
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, float> typePrintingConfig, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
        }
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this TypePrintingConfig<TOwner, string> typePrintingConfig, int length)
        {
            var iTPrintingConfig = (ITypePrintingConfig<TOwner>)typePrintingConfig;
            var printingConfig = iTPrintingConfig.PrintingConfig;
            var nameMember = iTPrintingConfig.NameMember;
            var trimmingFunc = new Func<string, string>(str => str.Length >= length ? str.Substring(0, length) : str);

            if (printingConfig.TrimmingFunctions.ContainsKey(nameMember))
                printingConfig.TrimmingFunctions[nameMember] = trimmingFunc;
            else printingConfig.TrimmingFunctions.Add(nameMember, trimmingFunc);

            return printingConfig;
        }
    }
}