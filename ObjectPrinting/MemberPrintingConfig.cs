using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TFieldType> : IMemberPrintingConfig<TOwner, TFieldType>
{
    private readonly PrintingConfig<TOwner> config;
    public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        config = printingConfig;
    }

    public IPrintingConfig<TOwner> Config => config;

    public IPrintingConfig<TOwner> With(Func<TFieldType, string> serializer)
    {
        config.AlternativeSerializationForType.Add(typeof(TFieldType), o => serializer((TFieldType)o));
        return config;
    }

    public IPrintingConfig<TOwner> With(Expression<Func<TOwner, TFieldType>> expression, Func<TFieldType, string> serializer)
    {
        config.AlternativeSerializationForProp.Add(PrintingConfig<TOwner>.GetPropDetails(expression), o => serializer((TFieldType)o));
        return config;
    }
}