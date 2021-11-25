using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace ObjectPrinting
{
    public record PrintingConfig
    {
        public ImmutableHashSet<Type> ExcludingTypes { get; init; } = ImmutableHashSet<Type>.Empty;
        public ImmutableHashSet<PropertyInfo> ExcludingProperties { get; init; } = ImmutableHashSet<PropertyInfo>.Empty;
        public ImmutableDictionary<Type, Func<object, string>> TypePrinting { get; init; } =
            ImmutableDictionary<Type, Func<object, string>>.Empty;
        public ImmutableDictionary<PropertyInfo, Func<PropertyInfo, string>> PropertyPrinting { get; init; } =
            ImmutableDictionary<PropertyInfo, Func<PropertyInfo, string>>.Empty;

        public List<Type> FinalTypes { get; init; } = new();
    }
}