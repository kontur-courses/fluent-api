using System;

namespace ObjectPrinting
{
    public static class MemberConfigExtensions
    {
        public static PrintingConfig<TOwner> WithBounds<TOwner>(
            this PropertyConfig<TOwner, string> memberPrintingConfig, int start, int end)
        {
            memberPrintingConfig.printingConfig.AddSerializationBounds(memberPrintingConfig.member, start, end);
            return memberPrintingConfig.printingConfig;
        }
    }
}