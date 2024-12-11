using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ObjectPrinting.Solved;

public record SerializationSettings( ImmutableHashSet<Type> ExcludedPropertyTypes,
    ImmutableDictionary<Type, Func<object, string>> AlternativeSerialization,
    ImmutableDictionary<MemberInfo, Func<object, string>> MethodsForSerializingPropertiesAndFields)
{
    public SerializationSettings() : this(ImmutableHashSet<Type>.Empty,
        ImmutableDictionary<Type, Func<object, string>>.Empty, 
        ImmutableDictionary<MemberInfo, Func<object, string>>.Empty)
    {
        
    }
}