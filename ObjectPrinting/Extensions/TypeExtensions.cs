using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectPrinting.PropertyOrField;

namespace ObjectPrinting.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<IPropertyOrField> GetFieldsAndProperties(this Type type, BindingFlags bindingAttr)
        {
            return type.GetFields(bindingAttr)
                .Select(x => new PropertyOrField.PropertyOrField(x))
                .Concat(type.GetProperties(bindingAttr).Select(x => new PropertyOrField.PropertyOrField(x)));
        }
    }
}