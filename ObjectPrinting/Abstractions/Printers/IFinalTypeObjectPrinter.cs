using System;
using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Abstractions.Printers;

public interface IFinalTypeObjectPrinter
{
    bool IsFinalType(Type type);
    bool TryPrintToString(object obj, [NotNullWhen(true)] out string? result);
}