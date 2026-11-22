using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Solved
{
    public class MemberAccessor
    {
        public string Name { get; }
        public Type MemberType { get; }
        private readonly PropertyInfo? property;
        private readonly FieldInfo? field;

        public MemberAccessor(PropertyInfo property)
        {
            Name = property.Name;
            MemberType = property.PropertyType;
            this.property = property;
            field = null;
        }

        public MemberAccessor(FieldInfo field)
        {
            Name = field.Name;
            MemberType = field.FieldType;
            property = null;
            this.field = field;
        }

        public object? GetValue(object obj)
            => property != null ? property.GetValue(obj) : field!.GetValue(obj);
    }

}
