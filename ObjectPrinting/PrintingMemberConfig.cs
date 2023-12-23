using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PrintingMemberConfig<TOwner, T> : PrintingConfig<TOwner>
{
    protected readonly Expression<Func<TOwner, T>> Member;

    public PrintingMemberConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, T>> member)
        : base(printingConfig)
    {
        Member = member;
    }
    
    public PrintingMemberConfig<TOwner, T> WithSerialization(Func<T, string> serializationFunc)
    {
        var memberName = PrintingMemberConfigUtils.GetFullMemberName(Member);
        CustomMemberSerialization[memberName] = obj => serializationFunc((T)obj);
        return this;
    }
}