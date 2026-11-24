using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting;

public static class TypeExtensions
{
    public static IEnumerable<MemberInfo> GetPrintableMembers(this Type type)
    {
        return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(member => member is FieldInfo
                             || member is PropertyInfo property
                             && property.GetMethod?.IsPublic == true
                             && property.GetIndexParameters().Length == 0);
    }
}