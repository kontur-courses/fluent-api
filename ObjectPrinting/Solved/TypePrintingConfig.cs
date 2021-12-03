using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting.Solved
{
    public class TypePrintingConfig<TOwner, TPropType>
    {
        private readonly BodyPrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(BodyPrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Func<object,string> alternativePrint = obj => print((TPropType)obj);
            printingConfig.AlternativePrinting[typeof(TPropType)] = alternativePrint;
            return printingConfig.PrintingConfig;
        }
    }
}