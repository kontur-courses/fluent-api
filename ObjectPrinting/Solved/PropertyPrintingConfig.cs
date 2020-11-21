using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        internal readonly string fullNameProp;
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> expression = null)
        {
            this.printingConfig = printingConfig;
            fullNameProp =  expression != null ? expression.GetFullNameProperty() : null;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var type = typeof(TOwner);
            if(type.GetMethod("ToString").GetParameters()
                .Where(p => p.ParameterType == typeof(IFormatProvider)).Any())
                printingConfig.AddCulture(typeof(TPropType), culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}