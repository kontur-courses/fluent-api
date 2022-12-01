using System;
using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static object GetMemberValue(this MemberInfo memberInfo, object forObject)
        {
            if (forObject == null)
                return null;
            
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        }
        
        public static object GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                default:
                    throw new NotImplementedException();
            }
        } 
    }
}