using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> memberSelector;
        
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> memberSelector = null)
        {
            this.printingConfig = printingConfig;
            this.memberSelector = memberSelector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberSelector == null)
            {
                ((IPrintingConfig)printingConfig).TypePrintingFunctions[typeof(TPropType)] = print;
            }
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig) printingConfig).TypeCultures[typeof(TPropType)] = culture;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}