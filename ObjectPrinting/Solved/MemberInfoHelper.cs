using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Solved;

public static class MemberInfoHelper
{
    public static string GetMemberNameFromExpression(Expression expression)
    {
        if (expression is MemberExpression memberSelector)
            return memberSelector.Member.Name;
        throw new ArgumentException($"Can't convert {expression} to MemberExpression.");
    }

    public static Type GetTypeFromMemberInfo(MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => throw new ArgumentException("Member is not a field or a property!")
        };
    }

    public static object? GetValueFromMemberInfo(object obj, MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
            _ => throw new ArgumentException("Member is not a field or a property!")
        };
    }
}