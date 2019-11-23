using System;
using System.Collections.Generic;

namespace ObjectPrinting.Configs.ConfigInterfaces
{
    internal interface IPrintingConfig<TOwner>
    {
        HashSet<Type> ExcludedTypes { get; }
        Dictionary<Type, Func<Type, string>> AlternativeSerializations { get;}
    }
}