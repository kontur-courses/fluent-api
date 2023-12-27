using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class DataMember
    {
        public DataMember(FieldInfo fieldInfo)
        {
            Name = fieldInfo.Name;
            GetValue = fieldInfo.GetValue;
            Type = fieldInfo.FieldType;
            MemberInfo = fieldInfo;
        }

        public DataMember(PropertyInfo property)
        {
            Name = property.Name;
            GetValue = property.GetValue;
            Type = property.PropertyType;
            MemberInfo = property;
        }

        public string Name { get; }
        public Type Type { get; }
        public Func<object, object> GetValue { get; }
        public MemberInfo MemberInfo { get; }
    }
}