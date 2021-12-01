using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class FormattableTypeConfig<T, TOwner> : BasicTypeConfig<T, TOwner>
            where T : IFormattable
    {
        internal FormattableTypeConfig(PrintingConfig<TOwner> container) : base(container)
        {
        }

        public FormattableTypeConfig<T, TOwner> WithCulture(CultureInfo cultureInfo)
        {
            Container.WithCulture<T>(cultureInfo);
            return this;
        }
    }
}