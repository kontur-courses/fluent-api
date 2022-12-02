using System.Globalization;
using ObjectPrinting.Core.Configs.Basics;

namespace ObjectPrinting.Core.Configs.Formattable
{
    public class FormattableTypeConfig<T, TOwner> : BasicTypeConfig<T, TOwner> where T : IFormattable
    {
        internal FormattableTypeConfig(PrintingConfig<TOwner> container) : base(container) { }

        public FormattableTypeConfig<T, TOwner> WithCulture(CultureInfo cultureInfo)
        {
            Container.WithCulture<T>(cultureInfo);
            return this;
        }
    }
}