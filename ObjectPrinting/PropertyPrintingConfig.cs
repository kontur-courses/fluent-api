using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo? property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo? property = null)
        {
            this.printingConfig = printingConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.SetFormatter(typeof(TPropType), property, value => print((TPropType)value));
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return Using(value => Convert.ToString(value, culture));
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}