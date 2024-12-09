using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ObjectPrinting.Constants;

public static class PrintingConfigConstants
{
    public static string NewLine => Environment.NewLine;
    public static string NullLine => "null";
    public static string CircularReference => "circular reference";
    public static readonly IReadOnlySet<Type> FinalTypes = new HashSet<Type>()
    {
        typeof(int), 
        typeof(double), 
        typeof(float), 
        typeof(string),
        typeof(DateTime), 
        typeof(TimeSpan), 
        typeof(Guid),
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(decimal),
        typeof(short),
        typeof(ushort),
        typeof(uint),
        typeof(long),
        typeof(ulong)
    };
}