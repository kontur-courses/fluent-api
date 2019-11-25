using System;
using System.Linq.Expressions;

namespace ObjectPrinting.Infrastructure
{
    public class PrintingConfigUtils
    {
        public static string GetPropertyNameFromMemberSelector<TOwner, TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpr)
            {
                return memberExpr.Member.Name;
            }

            return null;
        }
    }
}