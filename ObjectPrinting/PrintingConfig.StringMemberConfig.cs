using System.Reflection;

namespace ObjectPrinting;

public partial class PrintingConfig<TOwner>
{
    public class StringMemberConfig : BasicMemberConfig<string>
    {
        internal StringMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member)
        {
        }

        public StringMemberConfig WithTrimLength(int length)
        {
            Container.WithTrimLength(member, length);
            return this;
        }
    }
}
