using System;
using System.Reflection;

namespace ObjectPrinting.Common
{
    internal class PropertyObject : FieldPropertyObject
    {
        private readonly PropertyInfo propertyInfo;

        public override MemberInfo Info => propertyInfo;
        public override Type Type => propertyInfo.PropertyType;
        public override string Name => propertyInfo.Name;
        public override object Value { get; protected set; }

        public PropertyObject(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public override void SetValue(object parent)
        {
            Value = propertyInfo.GetValue(parent);
        }
    }
}