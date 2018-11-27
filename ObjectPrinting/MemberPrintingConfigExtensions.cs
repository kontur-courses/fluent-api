using System.Globalization;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        #region Using CultureInfo for numeric types

        public static PrintingConfig<TOwner> Using<TOwner>(
            this MemberPrintingConfig<TOwner, double> memberPrintingConfig,
            CultureInfo culture)
        {
            ((IMemberPrintingConfig<TOwner, double>)memberPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(double)] = culture;

            return ((IMemberPrintingConfig<TOwner, double>)memberPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this MemberPrintingConfig<TOwner, int> memberPrintingConfig,
            CultureInfo culture)
        {
            ((IMemberPrintingConfig<TOwner, int>)memberPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(int)] = culture;

            return ((IMemberPrintingConfig<TOwner, int>)memberPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this MemberPrintingConfig<TOwner, float> memberPrintingConfig,
            CultureInfo culture)
        {
            ((IMemberPrintingConfig<TOwner, float>)memberPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(float)] = culture;

            return ((IMemberPrintingConfig<TOwner, float>)memberPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this MemberPrintingConfig<TOwner, long> memberPrintingConfig,
            CultureInfo culture)
        {
            ((IMemberPrintingConfig<TOwner, long>)memberPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(long)] = culture;

            return ((IMemberPrintingConfig<TOwner, long>)memberPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this MemberPrintingConfig<TOwner, short> memberPrintingConfig,
            CultureInfo culture)
        {
            ((IMemberPrintingConfig<TOwner, short>)memberPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(short)] = culture;

            return ((IMemberPrintingConfig<TOwner, short>)memberPrintingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this MemberPrintingConfig<TOwner, decimal> memberPrintingConfig,
            CultureInfo culture)
        {
            ((IMemberPrintingConfig<TOwner, decimal>)memberPrintingConfig)
                .PrintingConfig
                .CustomCultures[typeof(decimal)] = culture;

            return ((IMemberPrintingConfig<TOwner, decimal>)memberPrintingConfig).PrintingConfig;
        }

        #endregion

        public static PrintingConfig<TOwner> Trim<TOwner>(
            this MemberPrintingConfig<TOwner, string> memberPrintingConfig,
            int value)
        {
            var extendedMemberPrintingConfig = ((IMemberPrintingConfig<TOwner, string>)memberPrintingConfig);
            var config = extendedMemberPrintingConfig.PrintingConfig;

            config.StringsTrimValues[extendedMemberPrintingConfig.MemberInfo] = value;

            return config;
        }
    }
}