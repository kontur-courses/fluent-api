using System;
using System.Reflection;

namespace ObjectPrinting
{
    internal class ElementInfo
    {
        internal readonly Func<object, object> GetValue;
        internal readonly MemberInfo MemberInfo;
        internal readonly string Name;
        internal readonly Type Type;

        internal ElementInfo(FieldInfo fieldInfo) :
            this(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.GetValue, fieldInfo)
        {
        }

        internal ElementInfo(PropertyInfo propertyInfo) :
            this(propertyInfo.Name, propertyInfo.PropertyType, propertyInfo.GetValue, propertyInfo)
        {
        }

        internal ElementInfo(string name, Type type, Func<object, object> getValue, MemberInfo memberInfo = null)
        {
            Name = name;
            Type = type;
            GetValue = getValue;
            MemberInfo = memberInfo;
        }
    }
}