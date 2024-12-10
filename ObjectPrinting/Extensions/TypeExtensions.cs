using System.Collections.Generic;

namespace ObjectPrinting.Extensions;

public static class TypeExtensions
{
    private static readonly HashSet<Type> finalTypes =
    [
        typeof(int),
        typeof(double),
        typeof(float),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Guid)
    ];

    public static bool IsFinal(this Type currentType) =>
        finalTypes.Contains(currentType);
}