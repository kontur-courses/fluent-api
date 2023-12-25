using ObjectPrinting.Contracts;
using System;

namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    public static string IntoString<T>(this T instance, Func<StringSerializer<T>, StringSerializer<T>> config)
    {
        return (config(new StringSerializer<T>()) as ISerializer).Serialize(instance!, 0);
    }

    public static string IntoString<T>(this T instance)
    {
        return (new StringSerializer<T>() as ISerializer).Serialize(instance!, 0);
    }
}