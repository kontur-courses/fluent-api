using System;
using System.Collections.Generic;

namespace ObjectPrinting.PrintingConfig
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> TypesPrintingMethods { get; }
        Dictionary<string, Func<object, string>> PropertiesPrintingMethods { get; }
    }
}