using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string? fieldName;

        public string? FieldName => fieldName;
        public PrintingConfig<TOwner> ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string? fieldName=null)
        {
            this.printingConfig = printingConfig;
            this.fieldName = fieldName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (fieldName != null)
            {
                printingConfig.AddNewFieldPrintingFunc(fieldName, TPropTypeObject => print((TPropType)TPropTypeObject));
            }
            else
            {
                printingConfig.AddNewTypePrintingFunc(typeof(TPropType), TPropTypeObject => print((TPropType)TPropTypeObject));
            }
            return printingConfig;
        }
    }
}