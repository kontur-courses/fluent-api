using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = "")
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Expression<Func<TPropType, string>> print)
        {
            if (propertyName != "")
            {
                ((IPrintingConfig<TOwner>) printingConfig).PrintMethodsForProperties[propertyName] =
                    prop => print.Compile().Invoke((TPropType) prop);
            }
            else
                ((IPrintingConfig<TOwner>)printingConfig).PrintMethodsForTypes[typeof(TPropType)] =
                    prop => print.Compile().Invoke((TPropType)prop);         
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}