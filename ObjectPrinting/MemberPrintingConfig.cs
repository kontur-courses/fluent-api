using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemType> : IMemberPrintingConfig<TOwner, TMemType>
    {
        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemType>.ParentConfig => printingConfig;

        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;
        private readonly SerializerRepository serializerRepository;

        public MemberPrintingConfig(
            PrintingConfig<TOwner> printingConfig,
            MemberInfo memberInfo,
            SerializerRepository serializerRepository
        )
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
            this.serializerRepository = serializerRepository;
        }

        public PrintingConfig<TOwner> Using(Func<TMemType, string> typePrint)
        {
            if (memberInfo is not null)
                serializerRepository.MemberSerializers.Add(memberInfo, typePrint);
            else
                serializerRepository.TypeSerializers.Add(typeof(TMemType), typePrint);
            return printingConfig;
        }
    }
}