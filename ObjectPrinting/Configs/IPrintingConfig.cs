using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        IEnumerable<Type> ExcludedTypes { get; }
        IEnumerable<PropertyInfo> ExcludedProperties { get; }
        IReadOnlyDictionary<Type, ITransformator> TypeTransformators { get; }
        IReadOnlyDictionary<PropertyInfo, ITransformator> PropertyTransformators { get; }
    }
}