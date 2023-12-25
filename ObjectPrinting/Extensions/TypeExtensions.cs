using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingAttr)
        {
            var members = new List<MemberInfo>();

            members.AddRange(type.GetFields(bindingAttr));
            members.AddRange(type.GetProperties(bindingAttr));

            return members;
        }
    }
}