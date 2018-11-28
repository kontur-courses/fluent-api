using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class ExpressionExtensions
    {
        public static MemberExpression CastToMemberExpression(this Expression body)
        {
            if (body is MemberExpression member)
            {
                return member;
            }

            throw new ArgumentException(
                @"Can't cast given lambda body to MemberExpression. 
You have to return property or field in lambda",
                nameof(body));
        }
    }
}