using System;
using System.Globalization;

namespace ObjectPrinting
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
            SettingsHolder.TypeSerilizers.Add(typeof(TPropType), obj => print((TPropType) obj));
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var SettingsHolder = printingConfig as IPrintingConfigurationHolder;
            SettingsHolder.CultureInfos.Add(typeof(TPropType), culture);
            return printingConfig;
        }
    }
}