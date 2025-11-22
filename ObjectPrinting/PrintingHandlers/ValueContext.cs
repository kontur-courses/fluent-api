using System;
using System.Collections.Generic;

namespace ObjectPrinting.PrintingHandlers
{
    internal sealed class ValueContext
    {
        public object? Value { get; set; }
        public Type? Type { get; set; }
        public string Path { get; init; } = "";
        public int Indent { get; init; }
        public PrintingSettings Settings { get; init; } = null!;
        public HashSet<object> Visited { get; init; } = null!;
    }
}