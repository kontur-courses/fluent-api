using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Extensions
{
    public static class ReflectExtensions
    {
        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this IReflect type) =>
            type
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);
    }
}