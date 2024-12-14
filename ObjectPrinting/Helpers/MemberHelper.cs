using System;
using System.Reflection;

namespace ObjectPrinting.Helpers;

public static class MemberHelper
{
    public static bool TryGetMemberValue(object declaringObj, MemberInfo memberInfo, out object value)
    {
        value = null!;
        if (!TryGetTypeAndValueOfMember(declaringObj, memberInfo, out var memberData)) 
            return false;
        
        value = memberData!.Value.memberValue;
        return true;
    }

    public static bool TryGetMemberType(object declaringObj, MemberInfo memberInfo, out Type type)
    {
        type = null!;
        if (!TryGetTypeAndValueOfMember(declaringObj, memberInfo, out var memberData)) 
            return false;
        
        type = memberData!.Value.memberType;
        return true;
    }
    
    public static bool TryGetTypeAndValueOfMember(
        object declaringObj,
        MemberInfo memberInfo,
        out (object memberValue, Type memberType)? memberData)
    {
        memberData = memberInfo switch
        {
            PropertyInfo propertyInfo => (propertyInfo.GetValue(declaringObj)!, propertyInfo.PropertyType),
            FieldInfo fieldInfo => (fieldInfo.GetValue(declaringObj)!, fieldInfo.FieldType),
            _ => null
        };
        
        return memberData != null;
    }
}