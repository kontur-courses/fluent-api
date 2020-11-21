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
        Dictionary<FieldInfo, Delegate> FieldSerialization { get; }
        HashSet<PropertyInfo> ExcludedProperties { get; }
        HashSet<FieldInfo> ExcludedFields { get; }
    }
}
