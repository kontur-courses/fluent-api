using System;
using System.Reflection;
using ObjectPrinting.Contracts;

namespace ObjectPrinting.Contexts
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IContextPrintingConfig<TOwner, TPropType>
    {
        private PropertyInfo Property { get; }
        private PrintingConfig<TOwner> PrintingConfig { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property)
        {
            Property = property;
            PrintingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var newConfig = (PrintingConfig as IPrintingConfig)
                .AddAlternativePrintingFor(Property, obj => print((TPropType) obj));
            return newConfig as PrintingConfig<TOwner>;
        }
    }
}