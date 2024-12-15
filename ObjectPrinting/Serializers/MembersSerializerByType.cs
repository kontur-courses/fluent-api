using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Serializers;

public class MembersSerializerByType : IMembersSerializer
{
    public Dictionary<Type, Func<object, string>> SerializerRules { get; } = new();
    
    public bool TrySerialize(
        object memberValue,
        Type memberType,
        MemberInfo memberInfo,
        out string serializedMemberInfo)
    {
        serializedMemberInfo = null!;
        if (!SerializerRules.TryGetValue(memberType, out var rule)) 
            return false;
        
        serializedMemberInfo = rule(memberValue);
        return true;

    }
}