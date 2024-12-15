using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static object? GetValue(this MemberInfo info, object obj)
        {
            return info.MemberType == MemberTypes.Property 
                ? (info as PropertyInfo).GetValue(obj)
                : info.MemberType == MemberTypes.Field 
                    ? (info as FieldInfo).GetValue(obj)
                    : null;
        }

        public static Type GetMemberType(this MemberInfo info)
        {
            return info.MemberType == MemberTypes.Property
                ? (info as PropertyInfo).PropertyType
                : (info as FieldInfo).FieldType;
        }
    }
}
