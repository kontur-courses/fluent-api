using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using ObjectPrinting.PrintingInterfaces;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        
        private readonly Type typeToPrinting;
        Type IPropertyPrintingConfig<TOwner, TPropType>.Type => typeToPrinting;
        
        private readonly PropertyInfo propertyToPrinting;
        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property => propertyToPrinting;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Type typeToPrinting)
        {
            this.printingConfig = printingConfig;
            this.typeToPrinting = typeToPrinting;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyToPrinting)
        {
            this.printingConfig = printingConfig;
            this.propertyToPrinting = propertyToPrinting;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var interfacedPrintingConfig = printingConfig as IPrintingConfig;
            if (interfacedPrintingConfig == null) return printingConfig;
            
            if (typeToPrinting!= null)
                interfacedPrintingConfig.TypeCustomSerializers[typeToPrinting] = print;
            else if (propertyToPrinting != null)
                interfacedPrintingConfig .PropertyCustomSerializers[propertyToPrinting] = print;

            return printingConfig;
        }
    }
}