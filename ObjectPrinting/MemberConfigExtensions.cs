
namespace ObjectPrinting
{
    public static class MemberConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this MemberConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.PrintAs(x => x[..^maxLen]);
        }
    }
}
