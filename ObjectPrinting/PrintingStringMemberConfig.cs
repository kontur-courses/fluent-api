using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PrintingStringMemberConfig<TOwner> : PrintingMemberConfig<TOwner, string>
{
    public PrintingStringMemberConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, string>> member) : base(printingConfig, member)
    {
    }
    
    public PrintingStringMemberConfig<TOwner> SetStringMaxLength(int length)
    {
        if (length < 0)
            throw new ArgumentException("Provided string length cannot be negative");

        var memberName = PrintingMemberConfigUtils.GetFullMemberName(Member);
        MemberMaxStringLength[memberName] = length;
        return this;
    }
}