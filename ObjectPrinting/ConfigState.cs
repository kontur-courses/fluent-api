using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ConfigState
    {
        public ImmutableHashSet<Type> ExcludedTypes { get; set; }
        public ImmutableHashSet<PropertyInfo> ExcludedProperties { get; set; }
        public ImmutableDictionary<Type, Delegate> AltSerializerForType { get; set; }
        public ImmutableDictionary<PropertyInfo, Delegate> AltSerializerForProperty { get; set; }
        public ImmutableDictionary<Type, CultureInfo> CultureForType { get; set; }

        public ConfigState()
        {
            ExcludedTypes = ImmutableHashSet.Create<Type>();
            ExcludedProperties = ImmutableHashSet.Create<PropertyInfo>();
            AltSerializerForType = ImmutableDictionary.Create<Type, Delegate>();
            AltSerializerForProperty = ImmutableDictionary.Create<PropertyInfo, Delegate>();
            CultureForType = ImmutableDictionary.Create<Type, CultureInfo>();
        }

        public ConfigState(ConfigState previousState)
        {
            ExcludedTypes = previousState.ExcludedTypes;
            ExcludedProperties = previousState.ExcludedProperties;
            AltSerializerForType = previousState.AltSerializerForType;
            AltSerializerForProperty = previousState.AltSerializerForProperty;
            CultureForType = previousState.CultureForType;
        }
    }
}
