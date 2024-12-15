using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Serializers;

public class MembersSerializerByMember : BaseMembersSerializer
{
    public Dictionary<MemberInfo, Func<object, string>> SerializerRules { get; } = new();
    
    protected override bool TrySerializeCore(
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