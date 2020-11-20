using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        HashSet<Type> ExcludedTypes { get; }
        Dictionary<Type, Delegate> TypeSerialization { get; }
        Dictionary<PropertyInfo, Delegate> PropertySerialization { get; }
        HashSet<PropertyInfo> ExcludedProperties { get; }
    }
}
