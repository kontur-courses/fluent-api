using System;
using System.Collections.Generic;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public string PropertyName { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            ParentConfig = printingConfig;
            PropertyName = propertyName;
        }

        public PrintingConfig<TOwner> WithConfig(Func<TPropType, string> config)
        {
            var parentConfig = (IPrintingConfig<TOwner>) ParentConfig;
            string PrintingMethod(object obj) => config((TPropType) obj);
            var propertyType = typeof(TPropType);

            if (PropertyName == null)
                AddOrUpdatePrintingMethods(parentConfig.TypesPrintingMethods, propertyType, PrintingMethod);
            else
                AddOrUpdatePrintingMethods(parentConfig.PropertiesPrintingMethods, PropertyName, PrintingMethod);

            return ParentConfig;
        }

        private static void AddOrUpdatePrintingMethods<T>(
            Dictionary<T, Func<object, string>> printingMethods, T elem, Func<object, string> printingMethod)
        {
            if (printingMethods.ContainsKey(elem))
                printingMethods.Add(elem, printingMethod);
            else printingMethods[elem] = printingMethod;
        }
    }
}