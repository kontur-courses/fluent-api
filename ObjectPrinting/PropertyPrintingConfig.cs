using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> serializationMap;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Dictionary<Type, Func<object, string>> serializationMap)
        {
            this.printingConfig = printingConfig;
            this.serializationMap = serializationMap;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;


        public PrintingConfig<TOwner> Using(Func<TPropType, string> method)
        {
            serializationMap[typeof(TPropType)] = arg => method((TPropType) arg);
            return printingConfig;
        }
    }

    internal interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }


    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> name,
            CultureInfo cultureInfo)
        {
            return name.Using(arg => arg.ToString(cultureInfo));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> name,
            CultureInfo cultureInfo)
        {
            return name.Using(arg => arg.ToString(cultureInfo));
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> name,
            CultureInfo cultureInfo)
        {
            return name.Using(arg => arg.ToString(cultureInfo));
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(this PropertyPrintingConfig<TOwner, string> number,
            int count)
        {
            return ((ITypePrintingConfig<TOwner>) number).PrintingConfig;
        }
    }
}