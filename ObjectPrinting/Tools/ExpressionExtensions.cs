using System;
using System.Linq.Expressions;

namespace ObjectPrinting.Tools;

public static class ExpressionExtensions
{
    public static string TryGetPropertyName<T1, T2>(this Expression<Func<T1, T2>> propertySelector) 
        => propertySelector.Body switch 
        { 
            MemberExpression memberExpression => memberExpression.GetFullName(), 
            UnaryExpression { Operand: MemberExpression operand } => operand.GetFullName(),
            
            _ => throw new ArgumentOutOfRangeException(nameof(propertySelector)) 
        };

    public static string GetFullName(this MemberExpression memberExpression)
        => $"{memberExpression.Member.ReflectedType!.FullName}.{memberExpression.Member.Name}";
}