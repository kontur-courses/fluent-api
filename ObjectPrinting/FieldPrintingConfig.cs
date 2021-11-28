using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class FieldPrintingConfig<TOwner, TField> : IMemberPrintingConfig<TOwner, TField>
    {
        private readonly FieldInfo fieldInfo;

        public PrintingConfig<TOwner> ParentConfig { get; }

        public FieldPrintingConfig(PrintingConfig<TOwner> parentConfig, FieldInfo fieldInfo)
        {
            ParentConfig = parentConfig;
            this.fieldInfo = fieldInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TField, string> serializationRule)
        {
            return new PrintingConfig<TOwner>(ParentConfig, fieldInfo, serializationRule);
        }
    }
}
