using System;
using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core.PropertyPrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingFunction)
        {
            var wrapper = WrapPrintingFunction(printingFunction);
            if (propertyName == null)
            {
                ((IPrintingConfig) printingConfig).TypePrintingFunctions[typeof(TPropType)] = wrapper;
            }
            else
            {
                ((IPrintingConfig) printingConfig).PropertyPrintingFunctions[propertyName] = wrapper;
            }

            return printingConfig;
        }

        private Func<object, string> WrapPrintingFunction(Func<TPropType, string> printingFunction)
        {
            return (obj) =>
            {
                if (obj is TPropType propType)
                {
                    return printingFunction(propType);
                }

                throw new ArgumentException($"Argument must be of type {nameof(TPropType)}");
            };
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => printingConfig;
    }
}