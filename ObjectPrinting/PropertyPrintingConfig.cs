using System;
using System.Globalization;
using System.Linq.Expressions;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string name;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            name = string.Empty;
            this.printingConfig = printingConfig;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            name = propertyName;
            this.printingConfig = printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (name == string.Empty)
                printingConfig.AddTypePrintingSettings(typeof(TPropType), print);
            else
            {
                printingConfig.AddPropertyPrintingSettings(name, print);
            }
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        string IPropertyPrintingConfig<TOwner, TPropType>.Name => name;

    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string Name { get; }
    }
}