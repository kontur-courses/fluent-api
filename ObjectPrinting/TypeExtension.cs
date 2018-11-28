using System;
using System.Linq;

namespace ObjectPrinting
{
    public static class TypeExtension
    {
        private static readonly Type[] ComplexSerializable =
        {
            typeof(string), typeof(TimeSpan), typeof(DateTime)
        };

        public static bool IsFinal(this Type type)
        {
            return type.IsPrimitive || type.IsEnum || ComplexSerializable.Contains(type);
        }
    }
}
