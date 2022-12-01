using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TField> : PrintingConfig<TOwner>
    {
        public readonly PrintingConfig<TOwner> parentConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            parentConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> With(Func<TField, string> config)
        {
            string PrintingMethod(object obj) => config((TField)obj);
            var propertyType = typeof(TField);

            if (propertyName == null)
            {
                parentConfig.MethodsForPrintingTypes[propertyType] = PrintingMethod;
            }
            else
            {
                parentConfig.MethodsForPrintingProperties[propertyName] = PrintingMethod;
            }

            return parentConfig;
        }
    }
}