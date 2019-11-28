using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Delegate> PrintingFunctionsByType { get; }
        Dictionary<string, Delegate> PrintingFunctionsByName { get; }
    }
}