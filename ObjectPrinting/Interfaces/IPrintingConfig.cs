using System;
using System.Collections.Generic;
using ObjectPrinting.Core;

namespace ObjectPrinting.Interfaces
{
    internal interface IPrintingConfig
    {
        Dictionary<Type, Delegate> AlternativeSerializationByTypes { get; }
        Dictionary<ElementInfo, Delegate> AlternativeSerializationByElementsInfo { get; }
    }
}