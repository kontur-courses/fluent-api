using System;
using System.Reflection;

namespace ObjectPrinting.Helpers;

public static class MemberHelper
{
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