using System;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting.Extensions;

public static class PrintingConfigExtensions
{
    internal static string GetPropertyName<TOwner, TProp>(Expression<Func<TOwner, TProp>> selector)
    {
        return selector.Body switch
        {
            MemberExpression m => m.Member.Name,
            UnaryExpression { Operand: MemberExpression m2 } => m2.Member.Name,
            _ => throw new ArgumentException("Expression must be a property")
        };
    }

    public static string PrintToString<TOwner>(this PrintingConfig<TOwner> printingConfig, object obj)
    {
        var sb = new StringBuilder();
        printingConfig.Visited.Clear();
        obj.Print(printingConfig, sb, 0);
        return sb.ToString();
    }
}