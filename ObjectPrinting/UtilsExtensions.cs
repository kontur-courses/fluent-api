using System;
using System.Linq;

namespace ObjectPrinting;

public static class UtilsExtensions
{
    private static readonly Type[] simpleTypes =
    [
        
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort),
        typeof(byte),
        typeof(sbyte),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(char),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Guid)
    ];

    public static bool IsSimple(this Type type) => simpleTypes.Contains(type);
}