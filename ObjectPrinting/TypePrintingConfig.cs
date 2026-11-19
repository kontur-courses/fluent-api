using System;

namespace ObjectPrinting
{
    public sealed class TypePrintingConfig<TRoot, TValue>
    {
        private readonly PrintingConfig<TRoot> printingConfig;

        internal TypePrintingConfig(PrintingConfig<TRoot> printingConfig)
        {
            ArgumentNullException.ThrowIfNull(printingConfig);

            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TRoot> Using(Func<TValue, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            printingConfig.Settings.SetTypeFormatter(
                typeof(TValue),
                value => formatter((TValue)value!));

            return printingConfig;
        }

        internal PrintingConfig<TRoot> UsingCulture(System.Globalization.CultureInfo culture)
        {
            printingConfig.Settings.SetTypeCulture(typeof(TValue), culture);

            return printingConfig;
        }
    }
}