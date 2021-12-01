using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting.Solved
{
    public class TypePrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType> 
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Func<object,string> newFunc = obj => print((TPropType)obj);
            ((IPrintingConfig<TOwner>)printingConfig).AlternativePrinting.Add(typeof(TPropType), newFunc);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    
}