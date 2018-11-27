using System;
using System.Globalization;

namespace ObjectPrinting
{

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, int> typePrintingConfig, CultureInfo culture)
        {
            var typeNumber = typeof(int);
            var printingConfig = ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
            SetCultureInfo(culture, typeNumber, printingConfig);
            return printingConfig;
        }

        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, double> typePrintingConfig, CultureInfo culture)
        {
            var typeNumber = typeof(double);
            var printingConfig = ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
            SetCultureInfo(culture, typeNumber, printingConfig);
            return printingConfig;
        }
        public static PrintingConfig<TOwner> SetCultureInfo<TOwner>(this TypePrintingConfig<TOwner, float> typePrintingConfig, CultureInfo culture)
        {
            var typeNumber = typeof(float);
            var printingConfig = ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
            SetCultureInfo(culture, typeNumber, printingConfig);
            return printingConfig;
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


        private static void SetCultureInfo<TOwner>(CultureInfo culture, Type typeNumber, PrintingConfig<TOwner> printingConfig)
        {
            if (printingConfig.CultureInfoForNumbers.ContainsKey(typeNumber))
                printingConfig.CultureInfoForNumbers[typeNumber] = culture;
            else printingConfig.CultureInfoForNumbers.Add(typeNumber, culture);
        }
    }
}