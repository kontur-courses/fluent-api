using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class MemberConfig<TOwner, TValue>
    {
        protected readonly PrintingConfig<TOwner> PrintingConfig;
        private protected readonly PrintingSettings Settings;
        protected readonly MemberInfo MemberInfo;

        internal MemberConfig(
            PrintingConfig<TOwner> printingConfig,
            PrintingSettings settings,
            MemberInfo memberInfo)
        {
            PrintingConfig = printingConfig;
            Settings = settings;
            MemberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(
            Func<TValue, string> serializer)
        {
            Settings.SetMemberSerializer(MemberInfo, serializer);
            return PrintingConfig;
        }
    }

    public sealed class StringMemberConfig<TOwner>
        : MemberConfig<TOwner, string>
    {
        internal StringMemberConfig(
            PrintingConfig<TOwner> printingConfig,
            PrintingSettings settings,
            MemberInfo memberInfo)
            : base(printingConfig, settings, memberInfo)
        {
        }

        public PrintingConfig<TOwner> Truncate(int maxLength)
        {
            Settings.SetMemberStringLimit(MemberInfo, maxLength);
            return PrintingConfig;
        }
    }
}
