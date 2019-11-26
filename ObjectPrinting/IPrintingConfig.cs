using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> PrintingFunctionsByType { get; }
        Dictionary<string, Func<object, string>> PrintingFunctionsByName { get; }
    }
}