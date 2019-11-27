using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Delegate> AlternativeSerializersForTypes { get; }
        Dictionary<string, Delegate> AlternativeSerializersForProperties { get; }
    }
}
