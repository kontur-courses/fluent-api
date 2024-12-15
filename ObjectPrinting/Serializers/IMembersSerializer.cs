using System;
using System.Reflection;

namespace ObjectPrinting.Serializers;

public interface IMembersSerializer
{
    public bool TrySerialize(object memberValue, Type memberType, MemberInfo memberInfo, out string result);
}