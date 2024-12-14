using System.Reflection;

namespace ObjectPrinting.Serializers;

public interface IMembersSerializer
{
    public bool TrySerialize(object obj, MemberInfo memberInfo, out string result);
}