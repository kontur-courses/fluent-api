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

        public MemberAccessor(PropertyInfo p)
        {
            Name = p.Name;
            MemberType = p.PropertyType;
            property = p;
            field = null;
        }

        public MemberAccessor(FieldInfo f)
        {
            Name = f.Name;
            MemberType = f.FieldType;
            property = null;
            field = f;
        }

        public object? GetValue(object obj)
            => property != null ? property.GetValue(obj) : field!.GetValue(obj);
    }

}
