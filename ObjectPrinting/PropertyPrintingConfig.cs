using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> serializationMap;
        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Dictionary<Type, Func<object, string>> serializationMap)
        {
            this.printingConfig = printingConfig;
            this.serializationMap = serializationMap;
        }
     

        public PrintingConfig<TOwner> Using(Func<TPropType, string> method)
        {
            serializationMap.Add(
                typeof(TPropType),
                arg => method((TPropType) arg));
            return printingConfig;
        }
    }

    internal interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> intName,
            CultureInfo cultureInfo)
        {
            return ((ITypePrintingConfig<TOwner>) intName).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> intName,
            CultureInfo cultureInfo)
        {
            return ((ITypePrintingConfig<TOwner>) intName).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> number,
            CultureInfo cultureInfo)
        {
            return ((ITypePrintingConfig<TOwner>) number).PrintingConfig;
        }

        public static PrintingConfig<TOwner> CutLast<TOwner>(this PropertyPrintingConfig<TOwner, string> number,
            int count)
        {
            return ((ITypePrintingConfig<TOwner>) number).PrintingConfig;
        }
    }
}