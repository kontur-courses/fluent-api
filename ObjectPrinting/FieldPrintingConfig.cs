using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class FieldPrintingConfig<TOwner>
    {
        private MemberInfo fieldInfo;
        private PrintingConfig<TOwner> context;

        public readonly List<MemberInfo> ExcludedFields = new List<MemberInfo>();

        public readonly Dictionary<MemberInfo, Func<object, string>> FieldSerialize
            = new Dictionary<MemberInfo, Func<object, string>>();

        public PrintingConfig<TOwner> SpecificSerialization(Func<object, string> serializer)
        {
            FieldSerialize.Add(fieldInfo, serializer);

            return context;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            ExcludedFields.Add(fieldInfo);

            return context;
        }

        public FieldPrintingConfig<TOwner> SwapContext(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
        {
            fieldInfo = memberInfo;
            context = printingConfig;

            return this;
        }
    }
}