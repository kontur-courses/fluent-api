using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo? fieldMember;

        public MemberInfo? FieldMember => fieldMember;
        public PrintingConfig<TOwner> ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo? fieldMember = null)
        {
            this.printingConfig = printingConfig;
            this.fieldMember = fieldMember;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (fieldMember != null)
            {
                printingConfig.AddNewFieldPrintingFunc(fieldMember.DeclaringType!, fieldMember.Name, TPropTypeObject => print((TPropType)TPropTypeObject));
            }
            else
            {
                printingConfig.AddNewTypePrintingFunc(typeof(TPropType), TPropTypeObject => print((TPropType)TPropTypeObject));
            }
            return printingConfig;
        }
    }
}