using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj,
                                              Func<PrintingConfig<T>, PrintingConfig<T>> config,
                                              int serialiseDepth = ObjectPrinter.DefaultSerialiseDepth,
                                              int sequencesMaxLength = ObjectPrinter.DefaultSequencesMaxLength) =>
            config(ObjectPrinter.For<T>(serialiseDepth, sequencesMaxLength)).PrintToString(obj);

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this MemberPrintingConfig<TOwner, string> memberConfig, int maxContentLength)
        {
            if (maxContentLength <= 0)
                throw new ArgumentException("Length has to be more than 0.", nameof(maxContentLength));

            var printingConfig = ((IMemberPrintingConfig<TOwner, string>)memberConfig).ParentConfig;

            ((IPrintingConfig)printingConfig).SetMaxValueLengthForStringMember(maxContentLength);

            return printingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this MemberPrintingConfig<TOwner, int> memberConfig,
                                                           CultureInfo cultureInfo) =>
            memberConfig.UsingCultureInfo(number => number.ToString(cultureInfo));

        public static PrintingConfig<TOwner> Using<TOwner>(this MemberPrintingConfig<TOwner, double> memberConfig,
                                                           CultureInfo cultureInfo) =>
            memberConfig.UsingCultureInfo(number => number.ToString(cultureInfo));

        public static PrintingConfig<TOwner> Using<TOwner>(this MemberPrintingConfig<TOwner, float> memberConfig,
                                                           CultureInfo cultureInfo) =>
            memberConfig.UsingCultureInfo(number => number.ToString(cultureInfo));

        private static PrintingConfig<TOwner> UsingCultureInfo<TOwner, TNumber>(
            this IMemberPrintingConfig<TOwner, TNumber> memberConfig, Func<TNumber, string> cultureApplier)
            where TNumber : struct
        {
            ((IPrintingConfig)memberConfig.ParentConfig).SetCultureInfoApplierForNumberType(cultureApplier);

            return memberConfig.ParentConfig;
        }
    }
}