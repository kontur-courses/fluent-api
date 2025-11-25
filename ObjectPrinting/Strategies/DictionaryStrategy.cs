using System;
using System.Collections;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Strategies;

public class DictionaryStrategy : IPrintStrategy
{
    public bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config)
        => obj is IDictionary;

    public void Print<TOwner>(object obj, Type type, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
        ((IDictionary)obj).PrintDictionary(config, sb, indent);
    }
}