using System;
using System.Collections.Generic;

namespace ObjectPrinting.Interfaces
{
    internal interface IPrintingConfig
    {
        Dictionary<Type, Delegate> AlternativeSerializationByTypes { get; }
        Dictionary<string, Delegate> AlternativeSerializationByNames { get; }
    }
}