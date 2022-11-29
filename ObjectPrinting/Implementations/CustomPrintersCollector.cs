using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ObjectPrinting.Abstractions;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations;

public class CustomPrintersCollector : ICustomPrintersCollector
{
    private readonly Dictionary<Type, ICustomPrinterProvider> _customTypesPrintingRules = new();

    private readonly Dictionary<IReadOnlyList<string>, ICustomPrinterProvider> _customMembersPrintingRules =
        new(new EnumerableEqualityComparer<string>());

    public void AddPrinterFor<T>(ICustomPrinterProvider printerProvider) =>
        AddPrinterFor(typeof(T), printerProvider);

    public void AddPrinterFor(Type type, ICustomPrinterProvider printerProvider) =>
        _customTypesPrintingRules[type] = printerProvider;

    public void AddPrinterFor(IReadOnlyList<string> memberPath, ICustomPrinterProvider printerProvider) =>
        _customMembersPrintingRules[memberPath] = printerProvider;

    public bool TryGetPrinterFor(PrintingMemberData memberData, [NotNullWhen(true)] out ICustomObjectPrinter? printer)
    {
        printer = default;
        if (!_customMembersPrintingRules.TryGetValue(memberData.MemberPath, out var provider) &&
            !_customTypesPrintingRules.TryGetValue(memberData.MemberType, out provider))
            return false;
        if (!provider.TryGetPrinter(out printer))
            throw new InvalidOperationException("Custom printer was not initialized before printing!");
        return true;
    }
}