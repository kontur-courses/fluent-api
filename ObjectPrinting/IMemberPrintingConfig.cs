using System;

namespace ObjectPrinting;

public interface IMemberPrintingConfig<TMember>
{
    PrintingConfig<TMember> PrintingConfig { get; }
    string Print(object obj);
}