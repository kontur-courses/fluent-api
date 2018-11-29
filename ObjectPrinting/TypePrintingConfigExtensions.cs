using System;
using System.Collections;
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
            var printingConfig = ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
            var nameMember = typePrintingConfig.GetNameMember();
            var trimmingFunc = new Func<string, string>(str => str.Length >= length ? str.Substring(0, length) : str);
            printingConfig.AddTrimmingFunctions(nameMember, trimmingFunc);
            return printingConfig;
        }

        private static void SetCultureInfo<TOwner>(CultureInfo culture, Type typeNumber, PrintingConfig<TOwner> printingConfig)
        {
            printingConfig.AddCultureInfoForNumbers(typeNumber, culture);
        }

        public static PrintingConfig<TOwner> AddMemberCountForInfiniteSequence<TOwner>
            (this TypePrintingConfig<TOwner, IEnumerable> typePrintingConfig, int numberIterations)
        {
            var printingConfig = ((ITypePrintingConfig<TOwner>)typePrintingConfig).PrintingConfig;
            printingConfig.AddNumberIterationsForEnumerable(numberIterations);
            return printingConfig;
        }
    }
}