using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class ReflectionHelpers
    {
        public static MemberInfo GetMemberFromExpression<TInput, TResult>
            (Expression<Func<TInput, TResult>> expression)
        {
            var body = expression.Body;

            var member = body as MemberExpression;

            if (member == null)
            {
                member = (expression.Body as UnaryExpression)?.Operand as MemberExpression;
            }
            if (member == null)
            {
                throw new ArgumentException("Action must be a member expression.");
            }

            return member.Member;
        }
    }
}
