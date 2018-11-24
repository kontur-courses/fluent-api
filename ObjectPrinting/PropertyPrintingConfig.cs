using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> field)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> method)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }

    interface ITypePrintingConfig<TOwner>
    { 
        PrintingConfig<TOwner> PrintingConfig { get; }
    }

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> intName, CultureInfo cultureInfo)
        {
            return ((ITypePrintingConfig<TOwner>)intName).PrintingConfig;
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> intName, CultureInfo cultureInfo)
        {
            return ((ITypePrintingConfig<TOwner>)intName).PrintingConfig;
        }
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> number, CultureInfo cultureInfo)
        {
            return ((ITypePrintingConfig<TOwner>)number).PrintingConfig;
        }
        public static PrintingConfig<TOwner> CutLast<TOwner>(this PropertyPrintingConfig<TOwner, string> number, int count)
        {
            return ((ITypePrintingConfig<TOwner>)number).PrintingConfig;
        }
    }
}