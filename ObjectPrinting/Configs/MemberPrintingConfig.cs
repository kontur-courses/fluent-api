using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Configs;

public class MemberPrintingConfig<TOwner, TProp>
{
    protected readonly PrintingContext context;
    private readonly MemberInfo memberInfo;
    private readonly PrintingConfig<TOwner> parent;

    public MemberPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo, PrintingContext context)
    {
        this.parent = parent;
        this.memberInfo = memberInfo;
        this.context = context;
    }

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        context.memberSerializers[memberInfo] = obj => serializer((TProp)obj);
        return parent;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        context.memberCultures[memberInfo] = culture;
        return parent;
    }
}