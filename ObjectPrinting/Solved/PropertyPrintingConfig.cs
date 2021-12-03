using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private BodyPrintingConfig<TOwner> printingConfigBody;
        public PropertyPrintingConfig(BodyPrintingConfig<TOwner> printingConfigBody)
        {
            this.printingConfigBody = printingConfigBody;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType,string> typePrintingConfig)
        {
            Func<object, string> alternativePrint = obj => typePrintingConfig((TPropType)obj);
            printingConfigBody
                .AlternativePrintingProp[printingConfigBody.PropertyKey] = alternativePrint;
            return printingConfigBody.PrintingConfig;
        }
    }
}
