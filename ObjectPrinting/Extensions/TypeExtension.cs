using System;
using System.Collections;

namespace ObjectPrinting
{
    public static class TypeExtension
    {
        public static bool IsDictionary(this Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        public static bool IsCollection(this Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type);
        }
    }
}
