using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting;

public static class PrintingMemberConfigUtils
{
    public static string GetFullMemberName<TOwner, T>(Expression<Func<TOwner, T>> expression)
    {
        PrintingConfigUtils.ValidateMemberExpression(expression);
        var expressionBody = expression.Body;

        var memberNames = new List<string>();
        while (expressionBody != null)
        {
            switch (expressionBody)
            {
                case MemberExpression memberExpression:
                    memberNames.Add(memberExpression.Member.Name);
                    expressionBody = memberExpression.Expression;
                    continue;
                case MethodCallExpression {Method.Name: "get_Item"} methodCallExpression:
                    memberNames.Add($"get_Item({methodCallExpression.Arguments[0]})");
                    expressionBody = methodCallExpression.Object;
                    continue;
                default:
                    expressionBody = null;
                    break;
            }
        }

        memberNames.Reverse();
        memberNames.Insert(0, typeof(TOwner).Name);

        return string.Join(".", memberNames);
    }
}