using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting;

public static class ReflectionExtensions
{
    public static object GetValue(this MemberInfo memberInfo, object obj)
    {
        return memberInfo switch
        {
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),
            _ => throw new Exception($"Cannot resolve member info type {memberInfo.GetType()}")
        };
    }

    public static Type GetMemberType(this MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            _ => throw new Exception($"Cannot resolve member info type {memberInfo.GetType()}")
        };
    }
    
    public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type)
    {
        return type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
    }
}