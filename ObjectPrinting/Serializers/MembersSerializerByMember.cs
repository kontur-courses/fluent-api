using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Serializers;

public class MembersSerializerByMember : IMembersSerializer
{
    public Dictionary<MemberInfo, Func<object, string>> SerializerRules { get; } = new();
    
    public bool TrySerialize(
        object memberValue,
        Type memberType,
        MemberInfo memberInfo,
        out string serializedMemberInfo)
    {
        serializedMemberInfo = null!;
        if (!SerializerRules.TryGetValue(memberInfo, out var rule)) 
            return false;
        
        serializedMemberInfo = rule(memberValue);
        return true;

    }
}