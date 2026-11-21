using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting;

internal class PrintingSettings
{
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<MemberInfo> excludedMembers = [];
    private readonly Dictionary<Type, Func<object, string>> typePrinters = [];
    private readonly Dictionary<MemberInfo, Func<object, string>> memberPrinters = [];

    public void ExcludeType(Type typeToExclude)
    {
        excludedTypes.Add(typeToExclude);
    }

    public void ExcludeMember(MemberInfo memberToExclude)
    {
        excludedMembers.Add(memberToExclude);
    }

    public void SetTypePrinter(Type type, Func<object, string> printer)
    {
        typePrinters[type] = printer;
    }

    public void SetMemberPrinter(MemberInfo member, Func<object, string> printer)
    {
        memberPrinters[member] = printer;
    }

    public bool IsExcluded(MemberInfo member, Type memberType)
    {
        return excludedMembers.Contains(member)
            || excludedTypes.Contains(memberType);
    }

    public bool TryGetTypePrinter(Type type, out Func<object, string> printer)
    {
        return typePrinters.TryGetValue(type, out printer);
    }

    public bool TryGetMemberPrinter(MemberInfo member, out Func<object, string> printer)
    {
        return memberPrinters.TryGetValue(member, out printer);
    }
}