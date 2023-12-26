using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class DataMember
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public Func<object, object> GetValue;
        public MemberInfo MemberInfo { get; set; }

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
    }
}
