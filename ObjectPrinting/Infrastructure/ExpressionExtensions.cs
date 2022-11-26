using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectPrinting.Infrastructure;

public static class ExpressionExtensions
{
    public static string[] GetMemberPath<TDeclaringType, TPropertyType>(
        this Expression<Func<TDeclaringType, TPropertyType>> expression
    )
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        var segments = new List<string>();
        Expression? node = expression;

        while (node is not null)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (node.NodeType)
            {
                case ExpressionType.Lambda:
                    node = ((LambdaExpression) node).Body;
                    break;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var unaryExpression = (UnaryExpression) node;
                    node = unaryExpression.Operand;
                    break;

                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) node;
                    node = memberExpression.Expression;

                    segments.Add(memberExpression.Member.Name);
                    break;

                case ExpressionType.Parameter:
                    node = null;
                    break;

                default:
                    throw new ArgumentException($"Expression <{expression}> cannot be used to select a member.");
            }
        }

        return segments.AsEnumerable().Reverse().ToArray();
    }
}