using System;
using System.Text;

namespace ObjectPrinting;

public interface IPrintStrategy
{
    bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config);
    void Print<TOwner>(object obj, Type type, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent);
}