using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public static class Helper
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            if (!(propertyLambda.Body is MemberExpression member))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));
            }

            if (!(member.Member is PropertyInfo propInfo))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));
            }

            Type type = typeof(TSource);
            if (propInfo.ReflectedType != null && type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));
            }

            return propInfo;
        }
    }
}