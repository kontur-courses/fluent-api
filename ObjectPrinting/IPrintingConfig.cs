using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> CustomTypesPrints { get; }
        Dictionary<string, Func<object, string>> CustomPropertiesPrints { get; }
    }
}
