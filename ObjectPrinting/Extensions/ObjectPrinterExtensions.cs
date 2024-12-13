using System;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting.Extensions;

public static class ObjectPrinterExtensions
{
    public static string? Serialize<TSerialize>(this IObjectPrinter printer, TSerialize obj) =>
        printer.For<TSerialize>().PrintToString(obj);

    public static string? Serialize<TSerialize>(this IObjectPrinter printer, TSerialize obj,
        Func<ObjectPrinterSettings<TSerialize>, ObjectPrinterSettings<TSerialize>> config) =>
        config(printer.For<TSerialize>()).PrintToString(obj);
}