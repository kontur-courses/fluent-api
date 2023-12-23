using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PrintingMemberConfigUtils
{
    public static string GetFullMemberName<TOwner, T>(Expression<Func<TOwner, T>> expression)
    {
        PrintingConfigUtils.ValidateMemberExpression(expression);
        var memberExpression = expression.Body as MemberExpression;
        
        var memberNames = new List<string>();
        while (memberExpression != null)
        {
            memberNames.Add(memberExpression.Member.Name);
            if (memberExpression.Expression is MemberExpression nestedMemberExpression)
                memberExpression = nestedMemberExpression;
            else
                memberExpression = null;
        }
        
        memberNames.Reverse();
        memberNames.Insert(0, typeof(TOwner).Name);
        return string.Join(".", memberNames);
    }
}