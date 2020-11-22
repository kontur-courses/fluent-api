using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            if (!(propertyLambda.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType!))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

            return propInfo;
        }
    }
}