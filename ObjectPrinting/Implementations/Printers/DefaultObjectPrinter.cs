using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Abstractions;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers;

public class DefaultObjectPrinter : IDefaultObjectPrinter
{
    public IExcludingRules ExcludingRules { get; }

    public DefaultObjectPrinter(IExcludingRules excludingRules)
    {
        ExcludingRules = excludingRules;
    }

    public string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootPrinter) =>
        memberData.MemberType.Name + rootPrinter.LineSplitter +
        string.Join(rootPrinter.LineSplitter, PrintToStringMembers(memberData, rootPrinter));

    private IEnumerable<string> PrintToStringMembers(PrintingMemberData memberData, IRootObjectPrinter rootPrinter)
    {
        var obj = memberData.Member;
        if (obj is null)
            throw new ArgumentNullException(nameof(memberData.Member));

        return obj.GetType().GetProperties()
            .Cast<MemberInfo>()
            .Concat(obj.GetType().GetFields())
            .AsParallel()
            .AsOrdered()
            .Select(member => memberData.CreateForChild(member.GetFieldPropertyValue(obj), member.Name))
            .Where(nestedMember => !ExcludingRules.IsExcluded(nestedMember))
            .Select(nestedMember => PrintMemberToString(nestedMember, rootPrinter.PrintToString(nestedMember)));
    }

    private static string PrintMemberToString(PrintingMemberData member, string value) =>
        $"{new string('\t', member.Nesting)}{member.MemberPath.Last()} = {value}";
}