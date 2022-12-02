using System.Reflection;
using System.Globalization;
using ObjectPrinting.Core.Configs.Basics;
using System.Diagnostics.Metrics;


namespace ObjectPrinting.Core.Configs.Formattable
{
    public class FormattableMemberConfig<T, TOwner> : BasicMemberConfig<T, TOwner> where T : IFormattable
    {
        internal FormattableMemberConfig(PrintingConfig<TOwner> container, MemberInfo member) : base(container, member) { }

        public FormattableMemberConfig<T, TOwner> WithCulture(CultureInfo cultureInfo)
        {
            Container.WithCulture(Member, cultureInfo);
            return this;
        }
    }
}
