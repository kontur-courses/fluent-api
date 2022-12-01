using System.Collections.Generic;
using System.Text;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers.SpecialPrinters;

public class KeyValueObjectPrinter : SpecialObjectPrinter
{
    private const char OpeningBrace = '{';
    private const char ClosingBrace = '}';

    public override bool CanPrint(object obj)
    {
        var type = obj.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    protected override string InternalPrintToString(PrintingMemberData memberData, IRootObjectPrinter rootPrinter)
    {
        var kvp = (dynamic) memberData.Member!;
        var key = (object) kvp.Key;
        var value = (object) kvp.Value;

        var keyStr = rootPrinter.PrintToString(memberData.CreateForChild(key, "Key"));
        var valueStr = rootPrinter.PrintToString(memberData.CreateForChild(value, "Value"));

        if (!keyStr.Contains(rootPrinter.LineSplitter) && !valueStr.Contains(rootPrinter.LineSplitter))
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