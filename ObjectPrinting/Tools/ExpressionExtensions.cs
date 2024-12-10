using System;
using System.Linq.Expressions;

namespace ObjectPrinting.Tools;

public static class ExpressionExtensions
{
    public static string TryGetPropertyName<T1, T2>(this Expression<Func<T1, T2>> propertySelector) 
        => propertySelector.Body switch 
        { 
            MemberExpression memberExpression => memberExpression.Member.Name, 
            UnaryExpression { Operand: MemberExpression operand } => operand.Member.Name,
            
            _ => throw new ArgumentOutOfRangeException(nameof(propertySelector)) 
        };
}