using System;
using System.Reflection;

namespace ObjectPrinting.Common
{
    internal class ValueObject : FieldPropertyObject
    {
        private readonly object obj;
        private readonly Type type;

        public override MemberInfo Info => null;
        public override Type Type => type;
        public override object Value
        {
            get => obj;
            protected set {}
        }
        public override string Name => string.Empty;

        public ValueObject(object value)
        {
            obj = value;
            type = value == null ? typeof(object) : value.GetType();
        }

        public override void SetValue(object parent)
        {
            return;
        }
    }
}