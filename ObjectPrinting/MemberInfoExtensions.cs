using System;
using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
            => memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => throw new NotImplementedException()
            };
        
        public static object GetValue(this MemberInfo memberInfo, object obj)
            => memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new NotImplementedException()
            };
    }
}