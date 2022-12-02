using System.Reflection;
using ObjectPrinting.Core.Configs.Basics;

namespace ObjectPrinting.Core.Configs.Defaults
{
    public class StringMemberConfig<TOwner> : BasicMemberConfig<string, TOwner>
    {
        internal StringMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member) { }

        public StringMemberConfig<TOwner> WithTrimLength(int length)
        {
            throw new NotImplementedException();
        }
    }
}