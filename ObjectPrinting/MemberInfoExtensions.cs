using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo member)
        {
            return member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new ArgumentException($"Member was not a property or field {member.MemberType}"),
            };
        }

        public static object GetValue(this MemberInfo member, object obj)
        {
            return member switch
            {
                PropertyInfo property => property.GetValue(obj),
                FieldInfo field => field.GetValue(obj),
                _ => throw new ArgumentException($"Member was not a property or field {member.MemberType}"),
            };
        }

        public static MemberInfo GetMemberInfoFromExpression<TOwner, T>(this Expression<Func<TOwner, T>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpr))
                throw new ArgumentException($"This is not member expression {memberSelector}");
            var propertyInfo = memberExpr.Member as PropertyInfo;
            var fieldInfo = memberExpr.Member as FieldInfo;
            if (propertyInfo == null && fieldInfo == null)
                throw new ArgumentException($"Wrong member selector {memberExpr}");
            return propertyInfo ?? (MemberInfo)fieldInfo;
        }
    }
}
