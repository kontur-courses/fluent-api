using System;
using System.Linq;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static bool IsSimpleType(this object obj)
        {
            if (obj == null)
                return true;
            var typeCode = Type.GetTypeCode(obj.GetType());
            var simpleTypes = Enum.GetNames(typeof(TypeCode));
            return typeCode != TypeCode.Object && simpleTypes.Contains(typeCode.ToString());
        }
    }
}