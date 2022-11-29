using System;
using ObjectPrinting.Abstractions.Printers;

namespace ObjectPrinting.Implementations.Printers;

public class CustomObjectPrinter<TMember> : ICustomObjectPrinter
{
    private readonly Func<TMember, string> _printFunc;

    public CustomObjectPrinter(Func<TMember, string> printFunc)
    {
        _printFunc = printFunc;
    }

    public string PrintToString(object obj)
    {
        if (obj is not TMember member)
            throw new ArgumentException($"{nameof(obj)} should be of type {typeof(TMember)}");
        return _printFunc(member);
    }
}