using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TFieldType> : IMemberPrintingConfig<TOwner, TFieldType>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly MemberInfo? info;
    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        config = printingConfig;
    }

    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo info)
    {
        config = printingConfig;
        this.info = info;
    }

    public IPrintingConfig<TOwner> Config => config;

    public IPrintingConfig<TOwner> With(Func<TFieldType, string> serializer)
    {
        if (info != null)
            config.AlternativeSerializationForProp.Add(info, o => serializer((TFieldType)o));
        else config.AlternativeSerializationForType.Add(typeof(TFieldType), o => serializer((TFieldType)o));
        return config;
    }
}