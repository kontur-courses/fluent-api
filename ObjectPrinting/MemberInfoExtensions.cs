using System;
using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(obj);
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(obj);
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public static Type GetReturnType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}