using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Helpers;

internal static class ReflectionHelper
{
    public static PropertyInfo GetPropertyInfoFromExpression<TOwner, TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression expression)
        {
            throw new ArgumentException("Expression must be a member expression", nameof(memberSelector));
        }

        if (expression.Expression == expression)
        {
            throw new ArgumentException("Cannot exclude the entire object. Use a property expression.",
                nameof(memberSelector));
        }

        if (expression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression must be a property expression", nameof(memberSelector));
        }

        return propertyInfo;
    }
}