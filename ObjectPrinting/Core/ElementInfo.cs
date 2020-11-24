using System;
using System.Reflection;

namespace ObjectPrinting.Core
{
    public class ElementInfo
    {
        public readonly Type ElementType;
        public readonly string NameElement;
        public readonly object Value;

        public ElementInfo(PropertyInfo propertyInfo, object obj)
        {
            ElementType = propertyInfo.PropertyType;
            NameElement = propertyInfo.Name;
            Value = propertyInfo.GetValue(obj);
        }

        public ElementInfo(FieldInfo fieldInfo, object obj)
        {
            ElementType = fieldInfo.FieldType;
            NameElement = fieldInfo.Name;
            Value = fieldInfo.GetValue(obj);
        }
    }
}