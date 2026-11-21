using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> parentConfig;
    private readonly MemberInfo? member;

    internal MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo? member = null)
    {
        this.parentConfig = parentConfig;
        this.member = member;
    }

    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        Func<object, string> boxedSerializer = obj => serializer((TProp)obj);
        return member is not null
            ? parentConfig.WithMemberSerializer(member, boxedSerializer)
            : parentConfig.WithTypeSerializer(typeof(TProp), boxedSerializer);
    }

    internal PrintingConfig<TOwner> WithCulture(CultureInfo culture) =>
        member is not null
            ? parentConfig.WithMemberCulture(member, culture)
            : parentConfig.WithTypeCulture(typeof(TProp), culture);

    internal PrintingConfig<TOwner> WithMaxLength(int maxLength) =>
        member is not null
            ? parentConfig.WithMemberMaxLength(member, maxLength)
            : parentConfig.WithStringMaxLength(maxLength);
}
