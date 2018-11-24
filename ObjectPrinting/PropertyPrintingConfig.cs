using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private Func<object, string> printer = o => 0.ToString();

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {

            printer = prop => print((TPropType) prop);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        Func<object, string> IPropertyPrintingConfig<TOwner, TPropType>.Printer =>
            printer;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Func<object, string> Printer { get; }
    }
}