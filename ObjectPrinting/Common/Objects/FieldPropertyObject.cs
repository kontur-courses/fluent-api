using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Common
{
    internal abstract class FieldPropertyObject
    {
        public abstract MemberInfo Info { get; }
        public abstract Type Type { get; }
        public abstract object Value { get; protected set; }
        public abstract string Name { get; }

        public abstract void SetValue(object parent);
    }
}