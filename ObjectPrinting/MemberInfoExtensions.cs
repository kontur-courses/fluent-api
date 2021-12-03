using System;
using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj)
            => memberInfo switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                Type type => obj,
                _ => throw new InvalidOperationException()
            };

        public static Type GetReturnType(this MemberInfo memberInfo)
            => memberInfo switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                Type type => type,
                _ => throw new InvalidOperationException()
            };
    }
}