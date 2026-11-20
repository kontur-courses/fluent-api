using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberConfig<TOwner, TValue>
    {
        protected readonly PrintingConfig<TOwner> PrintingConfig;
        protected readonly MemberInfo MemberInfo;

        internal MemberConfig(
            PrintingConfig<TOwner> printingConfig,
            MemberInfo memberInfo)
        {
            PrintingConfig = printingConfig;
            MemberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(
            Func<TValue, string> serializer)
        {
            PrintingConfig.SetMemberSerializer(MemberInfo, serializer);
            return PrintingConfig;
        }
    }

    public sealed class StringMemberConfig<TOwner>
        : MemberConfig<TOwner, string>
    {
        internal StringMemberConfig(
            PrintingConfig<TOwner> printingConfig,
            MemberInfo memberInfo)
            : base(printingConfig, memberInfo)
        {
        }

        public new PrintingConfig<TOwner> Using(
            Func<string, string> serializer)
        {
            base.Using(serializer);
            return PrintingConfig;
        }

        public PrintingConfig<TOwner> Truncate(int maxLength)
        {
            PrintingConfig.SetMemberStringLimit(MemberInfo, maxLength);
            return PrintingConfig;
        }
    }
}