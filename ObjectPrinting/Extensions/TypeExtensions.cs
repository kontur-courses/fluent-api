using System.Collections.Generic;

namespace ObjectPrinting.Extensions;

public static class TypeExtensions
{
    private static readonly HashSet<Type> FinalTypes =
    [
        typeof(int),
        typeof(double),
        typeof(float),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateTimeOffset)
    ];

    public static bool IsFinal(this Type currentType) =>
        FinalTypes.Contains(currentType);
}