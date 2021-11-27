using System;
using System.Globalization;

namespace ObjectPrinting;

public partial class PrintingConfig<TOwner>
{
    public class FormattableTypeConfig<T> : BasicTypeConfig<T>
        where T : IFormattable
    {
        internal FormattableTypeConfig(PrintingConfig<TOwner> container) : base(container)
        {
        }

        public FormattableTypeConfig<T> WithCulture(CultureInfo cultureInfo)
        {
            Container.WithCulture<T>(cultureInfo);
            return this;
        }
    }
}
