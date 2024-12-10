using System;
using System.Globalization;

namespace ObjectPrinting.Configs
{
    public class PropertyPrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig)
        : IPropertyPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}