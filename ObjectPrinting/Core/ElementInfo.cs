using System;
using System.Reflection;

namespace ObjectPrinting.Core
{
    public class ElementInfo
    {
        public readonly Type ElementType;
        public readonly string ElementName;
        private readonly Func<object, object> _getValueFunc;
        private readonly MemberInfo _memberInfo;

        public object GetValue(object instance) => _getValueFunc(instance);

        public ElementInfo(PropertyInfo propertyInfo)
        {
            ElementType = propertyInfo.PropertyType;
            ElementName = propertyInfo.Name;
            _getValueFunc = propertyInfo.GetValue;
            _memberInfo = propertyInfo;
        }

        public ElementInfo(FieldInfo fieldInfo)
        {
            ElementType = fieldInfo.FieldType;
            ElementName = fieldInfo.Name;
            _getValueFunc = fieldInfo.GetValue;
            _memberInfo = fieldInfo;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is ElementInfo elementInfo)
                return elementInfo._memberInfo == _memberInfo;
            return false;
        }

        public override int GetHashCode()
        {
            return _memberInfo.GetHashCode();
        }
    }
}