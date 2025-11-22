using System.Globalization;
using System.Reflection;

namespace Homework;

public interface IPrintingConfig
{
    HashSet<Type> ExcludeTypes { get; }
    HashSet<MemberInfo> ExcludeMembers { get; }
    Dictionary<Type, Func<object, string>> AlternativeTypesSerialization { get; }
    Dictionary<MemberInfo, Func<object, string>> AlternativeMembersSerialization { get; }
    Dictionary<Type, CultureInfo> TypesCultureInfo { get; }
    Dictionary<MemberInfo, int> StringsTrim { get; }
}
