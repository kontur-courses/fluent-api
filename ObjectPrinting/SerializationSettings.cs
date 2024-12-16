using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ObjectPrinting;

public record SerializationSettings(ImmutableHashSet<Type> ExcludedTypes,
    ImmutableHashSet<MemberInfo> ExcludedPropertiesAndFields,
    ImmutableDictionary<Type, Func<object, string?>> AlternativeTypeSerialization,
    ImmutableDictionary<MemberInfo, Func<object, string?>> AlternativeSerializationOfFieldsAndProperties)
{
    public SerializationSettings() : this(ImmutableHashSet<Type>.Empty,
        ImmutableHashSet<MemberInfo>.Empty,
        ImmutableDictionary<Type, Func<object, string?>>.Empty, 
        ImmutableDictionary<MemberInfo, Func<object, string?>>.Empty)
    {
        
    }
}