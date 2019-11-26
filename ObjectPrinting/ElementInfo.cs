using System;
using System.Reflection;

namespace ObjectPrinting
{
    internal class ElementInfo
    {
        internal readonly Func<object, object> GetValue;
        internal readonly MemberInfo MemberInfo;
        internal readonly Type Type;

        internal ElementInfo(FieldInfo fieldInfo) : this(fieldInfo as MemberInfo)
        {
            Type = fieldInfo.FieldType;
            GetValue = fieldInfo.GetValue;
        }

        internal ElementInfo(PropertyInfo propertyInfo) : this(propertyInfo as MemberInfo)
        {
            Type = propertyInfo.PropertyType;
            GetValue = propertyInfo.GetValue;
        }

        private ElementInfo(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }
    }
}