using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;
        private readonly List<Func<object, string>> printMethods = new List<Func<object, string>>();


        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Using(Func<TPropType, string> printMethod)
        {
           printMethods.Add(val => printMethod.Invoke((TPropType)val));
           return this;
        }

        public PrintingConfig<TOwner> Apply()
        {
            if(propertyName != null)
                ((IPrintingConfig<TOwner>)printingConfig).PrintMethodsForProperties[propertyName] = printMethods;
            else
                ((IPrintingConfig<TOwner>) printingConfig).PrintMethodsForTypes[typeof(TPropType)] = printMethods;
            return printingConfig;
        }

        List<Func<object, string>> IPropertyPrintingConfig<TPropType>.PrintMethods => printMethods;
    }

    interface IPropertyPrintingConfig<TPropType>
    {
        List<Func<object, string>> PrintMethods { get; }
    }
}