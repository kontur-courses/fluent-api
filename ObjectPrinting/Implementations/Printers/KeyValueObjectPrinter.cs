using System;
using System.Collections.Generic;
using System.Text;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers;

public class KeyValueObjectPrinter : ISpecialObjectPrinter
{
    private const char OpeningBrace = '{';
    private const char ClosingBrace = '}';

    public bool CanPrint(object obj)
    {
        var type = obj.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    public string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootObjectPrinter)
    {
        var obj = memberData.Member;
        if (obj is null || !CanPrint(obj))
            throw new ArgumentException("Unable to print this member, using this printer!");
        var kvp = (dynamic) obj;
        var key = (object) kvp.Key;
        var value = (object) kvp.Value;

        var keyStr = rootObjectPrinter.PrintToString(memberData.CreateForChild(key, "Key"));
        var valueStr = rootObjectPrinter.PrintToString(memberData.CreateForChild(value, "Value"));

        if (!keyStr.Contains(Environment.NewLine) && !valueStr.Contains(Environment.NewLine))
            return $"{OpeningBrace}{keyStr}: {valueStr}{ClosingBrace}";

        return JoinInMultiline(keyStr, valueStr, memberData.Nesting);
    }

    private static string JoinInMultiline(string key, string value, int nesting)
    {
        return new StringBuilder()
            .Append(OpeningBrace).AppendLine()
            .Append('\t', nesting + 1).Append(key).Append(':').AppendLine()
            .Append('\t', nesting + 1).Append(value).AppendLine()
            .Append('\t', nesting).Append(ClosingBrace)
            .ToString();
    }
}