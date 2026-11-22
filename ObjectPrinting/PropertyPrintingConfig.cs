using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>(
        PrintingConfig<TOwner> printingConfig,
        PropertyInfo? property = null) : IPropertyPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.SetFormatter(typeof(TPropType), property, value => print((TPropType)value));
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return Using(value =>
                value is IFormattable f ? f.ToString(null, culture) : value?.ToString() ?? string.Empty);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}