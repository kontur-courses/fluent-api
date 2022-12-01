using System;
using System.Reflection;

namespace ObjectPrinting.Common
{
    internal class FieldObject : FieldPropertyObject
    {
        private readonly FieldInfo fieldInfo;

        public override MemberInfo Info => fieldInfo;
        public override Type Type => fieldInfo.FieldType;

        public override object Value { get; protected set; }

        public override string Name => fieldInfo.Name;

        public FieldObject(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }

        public override void SetValue(object parent)
        {
            Value = fieldInfo.GetValue(parent);
        }
    }
}