using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class Member
    {
        public string Name;
        public Type Type;
        public object Value;

        public Member(MemberInfo memberInfo, object obj)
        {
            if (!(memberInfo is PropertyInfo || memberInfo is FieldInfo))
                throw new ArgumentException("MemberInfo is not FieldInfo or PropertyInfo");
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
            }
        }
    }
}
