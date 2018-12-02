using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class ClassMemberInfo
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly object Value;

        public ClassMemberInfo(Type type, string name, object value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

        public ClassMemberInfo(object obj, PropertyInfo info) : 
            this(info.PropertyType, info.Name, info.GetValue(obj)) { }

        public ClassMemberInfo(object obj, FieldInfo info) :
            this(info.FieldType, info.Name, info.GetValue(obj))
        { }
    }
}