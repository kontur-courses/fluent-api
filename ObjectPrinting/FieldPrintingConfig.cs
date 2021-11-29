using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class FieldPrintingConfig<TOwner, TField> : IMemberPrintingConfig<TOwner, TField>
    {
        private readonly FieldInfo fieldInfo;

        private PrintingConfig<TOwner> parentConfig;

        public FieldPrintingConfig(PrintingConfig<TOwner> parentConfig, FieldInfo fieldInfo)
        {
            this.parentConfig = parentConfig;
            this.fieldInfo = fieldInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TField, string> serializationRule)
        {
            return new PrintingConfig<TOwner>(parentConfig, fieldInfo, serializationRule);
        }
    }
}
