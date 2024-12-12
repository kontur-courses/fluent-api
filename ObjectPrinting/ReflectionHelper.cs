using System.Linq.Expressions;
using System;
using System.Reflection;
public static class ReflectionHelper
{
    public static PropertyInfo GetProperty<TOwner, TPropType>(Expression<Func<TOwner, TPropType>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }
            throw new ArgumentException("Expression is not a property", nameof(propertyExpression));
        }
        throw new ArgumentException("Expression is not a MemberExpression.", nameof(propertyExpression));
        //if (propertyExpression.Body is MemberExpression member)
        //{
        //    var property = member as PropertyInfo;
        //    //var className = typeof(TOwner).Name;
            
        //    var memberName = member.Member.Name;
        //    return memberName;
        //    //return $"{className} {memberName}";
        //}
            

        //throw new ArgumentException("Expression is not a property.");
    }
}
