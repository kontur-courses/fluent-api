using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TType> : IInnerTypeConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;
        private readonly SerializerSettings serializerSettings;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo,
            SerializerSettings serializerSettings)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
            this.serializerSettings = serializerSettings;
        }
        
        public PrintingConfig<TOwner> Using(Func<TType, string> serializer)
        {
            serializerSettings.AddMemberSerializer(memberInfo, serializer);
            return printingConfig;
        }
    }
}