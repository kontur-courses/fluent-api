using System.Reflection;

namespace ObjectPrinting
{
    public class StringMemberConfig<TOwner> : BasicMemberConfig<string, TOwner>
    {
        internal StringMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member)
        {
        }

        public StringMemberConfig<TOwner> WithTrimLength(int length)
        {
            Container.WithTrimLength(member, length);
            return this;
        }
    }
}