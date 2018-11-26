using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo prop;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo prop = null)
        {
            this.prop = prop;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Console.WriteLine("prop: " + prop);

            if (prop is null)
                printingConfig.TypesToBeAlternativelySerialized.Add(typeof(TPropType), print);
            else
                printingConfig.PropertiesToBeAlternativelySerialized.Add(prop.Name, print);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.NumericTypesToBeAlternativelySerializedUsingCultureInfo
                .Add(typeof(TPropType), culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}