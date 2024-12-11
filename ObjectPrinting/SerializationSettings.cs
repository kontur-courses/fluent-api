using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ObjectPrinting.Solved;

public record SerializationSettings(ImmutableHashSet<Type> ExcludedPropertyTypes,
    ImmutableHashSet<MemberInfo> ExcludedPropertiesAndFields,
    ImmutableDictionary<Type, Func<object, string>> AlternativeSerialization,
    ImmutableDictionary<MemberInfo, Func<object, string>> MethodsForSerializingPropertiesAndFields)
{
    public SerializationSettings() : this(ImmutableHashSet<Type>.Empty,
        ImmutableHashSet<MemberInfo>.Empty,
        ImmutableDictionary<Type, Func<object, string>>.Empty, 
        ImmutableDictionary<MemberInfo, Func<object, string>>.Empty)
    {
        
    }
}