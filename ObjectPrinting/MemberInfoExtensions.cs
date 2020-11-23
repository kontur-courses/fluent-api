using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        }

        public static object GetValueType(this MemberInfo memberInfo)
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
