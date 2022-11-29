using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Printers;

public class EnumerableObjectPrinter : ISpecialObjectPrinter
{
    private const char OpeningBrace = '[';
    private const char ClosingBrace = ']';

    public bool CanPrint(object obj) => obj is IEnumerable;

    public string PrintToString(PrintingMemberData memberData, IRootObjectPrinter rootObjectPrinter)
    {
        if (memberData.Member is not IEnumerable enumerable)
            throw new ArgumentException("Unable to print this member, using this printer!");

        var printed = PrintElementsToString(enumerable, rootObjectPrinter, memberData);

        return printed.All(obj => !obj.Contains(Environment.NewLine))
            ? $"{OpeningBrace}{string.Join(", ", printed)}{ClosingBrace}"
            : JoinInMultiline(memberData.Nesting, printed);
    }

    private static ICollection<string> PrintElementsToString(
        IEnumerable enumerable,
        IRootObjectPrinter rootObjectPrinter,
        PrintingMemberData memberData
    ) =>
        enumerable
            .AsParallel()
            .AsOrdered()
            .Cast<object?>()
            .Select((obj, i) => memberData.CreateForChild(obj, $"[{i}]"))
            .Select(rootObjectPrinter.PrintToString)
            .ToArray();

    private static string JoinInMultiline(int nesting, IEnumerable<string> printed)
    {
        var builder = new StringBuilder()
            .Append(OpeningBrace).AppendLine();
        foreach (var str in printed)
            builder
                .Append('\t', nesting + 1)
                .Append(str)
                .AppendLine();

        return builder.Append('\t', nesting).Append(ClosingBrace).ToString();
    }
}