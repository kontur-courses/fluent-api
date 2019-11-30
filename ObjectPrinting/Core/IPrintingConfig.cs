using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        HashSet<Type> ExcludingTypes { get; }
        HashSet<string> ExcludingProperties { get; }
        Dictionary<Type, ISerializerConfig<TOwner>> TypeSerializerConfigs { get; }
        Dictionary<string, ISerializerConfig<TOwner>> PropertySerializerConfigs { get; }
        string Print(object obj, int nestingLevel);
    }
}