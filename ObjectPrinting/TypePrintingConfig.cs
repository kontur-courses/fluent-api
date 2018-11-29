using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class TypePrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner, TPropType>.ParentConfig
        {
            get { return printingConfig; }
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var SettingsHolder = printingConfig as IPrintingConfigurationHolder;
            SettingsHolder.typeSerilizers.Add(typeof(TPropType), print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var SettingsHolder = printingConfig as IPrintingConfigurationHolder;
            SettingsHolder.cultureInfos.Add(typeof(TPropType), culture);
            return printingConfig;
        }
    }
}