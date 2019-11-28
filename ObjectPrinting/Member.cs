using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class Member
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly object Value;

        public Member(MemberInfo memberInfo, object obj)
        {
            Name = memberInfo.Name;
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    Type = propertyInfo.PropertyType;
                    Value = propertyInfo.GetValue(obj);
                    break;
                case FieldInfo fieldInfo:
                    Type = fieldInfo.FieldType;
                    Value = fieldInfo.GetValue(obj);
                    break;
                default:
                    throw new ArgumentException("MemberInfo is not FieldInfo or PropertyInfo");
            }
        }
    }
}
