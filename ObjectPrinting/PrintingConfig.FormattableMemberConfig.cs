using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public partial class PrintingConfig<TOwner>
{
    public class FormattableMemberConfig<T> : BasicMemberConfig<T>
        where T : IFormattable
    {
        internal FormattableMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member)
        {
        }

        public FormattableMemberConfig<T> WithCulture(CultureInfo cultureInfo)
        {
            Container.WithCulture<T>(member, cultureInfo);
            return this;
        }
    }
}
