using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private PrintingConfig<TOwner> printingConfig;
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        public PrintingConfig<TOwner> ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType,string> typePrintingConfig)
        {
            printingConfig.AddFuncForProp(obj => typePrintingConfig((TPropType)obj));
            return printingConfig;
        }
    }
}
