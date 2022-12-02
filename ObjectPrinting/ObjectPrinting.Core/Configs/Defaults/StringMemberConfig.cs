using System.Reflection;
using ObjectPrinting.Core.Configs.Basics;

namespace ObjectPrinting.Core.Configs.Defaults
{
    public class StringMemberConfig<TOwner> : BasicMemberConfig<string, TOwner>
    {
        internal StringMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member) { }

        public StringMemberConfig<TOwner> TrimEnd(int length)
        {
            Container.TrimEnd(Member, length);
            return this;
        }
    }
}