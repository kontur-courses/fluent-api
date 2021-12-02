using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializationMemberInfo
    {
        public string MemberName { get; }
        public Type MemberType { get; }
        public object MemberValue { get; }

        public SerializationMemberInfo(PropertyInfo property, object obj)
        {
            MemberName = property.Name;
            MemberType = property.PropertyType;
            MemberValue = property.GetValue(obj);
        }

        public SerializationMemberInfo(FieldInfo field, object obj)
        {
            MemberName = field.Name;
            MemberType = field.FieldType;
            MemberValue = field.GetValue(obj);
        }
    }
}
