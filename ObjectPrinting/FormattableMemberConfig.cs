using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class FormattableMemberConfig<T, TOwner> : BasicMemberConfig<T, TOwner>
            where T : IFormattable
    {
        internal FormattableMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member)
        {
        }

        public FormattableMemberConfig<T, TOwner> WithCulture(CultureInfo cultureInfo)
        {
            Container.WithCulture<T>(member, cultureInfo);
            return this;
        }
    }
}