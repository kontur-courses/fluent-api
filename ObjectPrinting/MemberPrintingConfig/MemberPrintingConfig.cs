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
}
