using System;
using System.Globalization;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

public class FormattableMemberConfig<TOwner, TProp> :
    IFormattableMemberConfig<TOwner, TProp>
    where TProp : IFormattable
{
    private readonly PrintingConfig<TOwner> config;
    private readonly string memberName;

    public FormattableMemberConfig(PrintingConfig<TOwner> config, string memberName)
    {
        this.config = config;
        this.memberName = memberName;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        config.PropertySerializers[memberName] = obj =>
            ((IFormattable)obj).ToString(null, culture);
        return config;
    }
}
