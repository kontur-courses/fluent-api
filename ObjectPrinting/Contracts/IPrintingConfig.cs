using System;

namespace ObjectPrinting.Contracts
{
    public interface IPrintingConfig
    {
        AlternativePrinter AlternativePrinter { get; }
        PrintExcluder PrintExcluder { get; }

        IPrintingConfig AddAlternativePrintingFor<TContext>(TContext entity, Func<object, string> print);
    }
}