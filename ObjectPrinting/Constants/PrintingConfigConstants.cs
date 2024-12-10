using System;
using System.Collections.Generic;

namespace ObjectPrinting.Constants;

public static class PrintingConfigConstants
{
    public static char Tab => '\t';
    public static string NewLine => Environment.NewLine;
    public static string NullLine => "null";
    public static string CircularReference => "circular reference";

    public static readonly IReadOnlySet<Type> CoreTypes = new HashSet<Type>()
    {
        typeof(string),
    };
}