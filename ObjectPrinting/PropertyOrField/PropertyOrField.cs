using System;
using System.Reflection;

namespace ObjectPrinting.PropertyOrField
{
    public class PropertyOrField : IPropertyOrField
    {
        public string Name { get; }
        public Type DataType { get; }
        public MemberInfo UnderlyingMember { get; }
        public Func<object, object> GetValue { get; }

        public PropertyOrField(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            DataType = propertyInfo.PropertyType;
            UnderlyingMember = propertyInfo;
            GetValue = propertyInfo.GetValue;
        }

        public PropertyOrField(FieldInfo fieldInfo)
        {
            Name = fieldInfo.Name;
            DataType = fieldInfo.FieldType;
            UnderlyingMember = fieldInfo;
            GetValue = fieldInfo.GetValue;
        }
    }
}