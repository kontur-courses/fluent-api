using System.Globalization;

namespace ObjectPrinting
{
    public class CulturePrintingConfig<TOwner>
    {
        private CultureInfo cultureInfo;
        private PrintingConfig<TOwner> printingConfig;

        public CulturePrintingConfig(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
        }

        public PrintingConfig<TOwner> For<T>()
        {
            var newConfig = printingConfig.AddConfig(typeof(T), o => o.ToString(cultureInfo))
            return new PrintingConfig<TOwner>();
        }
    }
}