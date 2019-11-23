using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Configs.ConfigInterfaces
{
    internal interface IPrintingConfig<TOwner>
    {
        HashSet<Type> ExcludedTypes { get; }
        Dictionary<Type, Func<object, string>> AlternativeSerializations { get;}
        HashSet<PropertyInfo> ExcludedProperties { get;}
    }
}