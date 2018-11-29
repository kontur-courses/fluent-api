using System;

namespace ObjectPrinting.Config.Member
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner> (this MemberPrintingConfig<TOwner, string> memberConfig, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException("Trimming length can't be negative");

            var parentConfig = ((IPrintingConfig<TOwner, string>) memberConfig).ParentConfig;
            Func<object, string> trimFunction = obj =>
            {
                var str = (string) obj;
                return ((string) obj).Substring(0, Math.Min(str.Length, maxLen));
            };
            parentConfig.OverrideMember(memberConfig.MemberToChange, trimFunction);

            return parentConfig;
        }
    }
}
