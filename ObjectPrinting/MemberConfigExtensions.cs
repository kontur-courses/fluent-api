namespace ObjectPrinting
{
    public static class MemberConfigExtensions
    {
        public static PrintingConfig<TOwner> WithBounds<TOwner>(
            this MemberConfig<TOwner, string> memberPrintingConfig, int start, int end)
        {
            memberPrintingConfig.PrintingConfig.AddSerializationBounds(memberPrintingConfig.Member, start, end);
            return memberPrintingConfig.PrintingConfig;
        }
    }
}