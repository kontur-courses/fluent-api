using System;
using System.Collections.Generic;

namespace ObjectPrinting;

public static class TypeExtensions
{
    public static bool IsNumericType(this Type type)
    {
        if (type == null)
            return false;
        
        HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(decimal), typeof(double), typeof(float)
        };
        
        Type underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
            return NumericTypes.Contains(underlyingType);

        return NumericTypes.Contains(type);
    }
}