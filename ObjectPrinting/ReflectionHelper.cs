using System.Linq.Expressions;
using System;
public static class ReflectionHelper
{
    public static string GetPropertyName<TOwner, TPropType>(Expression<Func<TOwner, TPropType>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException("Expression is not a property.");
    }
}
