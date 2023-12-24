using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public static class PrintingConfigUtils
{
    public static void ValidateMemberExpression<TOwner, T>(Expression<Func<TOwner, T>> member)
    {
        if (member.Body.NodeType != ExpressionType.MemberAccess)
            throw new MissingMemberException("Type's member has to be selected.");
    }
}