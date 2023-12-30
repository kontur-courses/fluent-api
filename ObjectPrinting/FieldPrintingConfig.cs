using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class FieldPrintingConfig<TOwner>
    {
        private readonly MemberInfo fieldInfo;
        private readonly PrintingConfig<TOwner> context;

        private readonly List<MemberInfo> excludedFields;

        private readonly Dictionary<MemberInfo, Func<object, string>> fieldsSerialize;

        public FieldPrintingConfig(
            List<MemberInfo> excludedFields, Dictionary<MemberInfo, Func<object, string>> fieldsSerialize,
            PrintingConfig<TOwner> printingConfig, MemberInfo fieldInfo)
        {
            this.excludedFields = excludedFields;
            this.fieldsSerialize = fieldsSerialize;
            context = printingConfig;
            this.fieldInfo = fieldInfo;
        }

        public PrintingConfig<TOwner> SpecificSerialization(Func<object, string> serializer)
        {
            fieldsSerialize.Add(fieldInfo, serializer);
            return context;
        }
        
        public PrintingConfig<TOwner> Exclude()
        {
            excludedFields.Add(fieldInfo);
            return context;
        }
    }
}