using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public static class ExpressionExtensions
{
    public static MemberInfo GetMemberInfo<TOwner, TMember>(
        this Expression<Func<TOwner, TMember>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var body = selector.Body;
    
        if (body is UnaryExpression unary &&
            unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;
    
        if (body is not MemberExpression memberExpr ||
            memberExpr.Member is not (PropertyInfo or FieldInfo) ||
            memberExpr.Expression is not ParameterExpression)
        {
            throw new ArgumentException(
                "Ожидается прямое обращение к полю или свойству объекта",
                nameof(selector));
        }
    
        return memberExpr.Member;
    }
}