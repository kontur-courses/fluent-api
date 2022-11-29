using System;
using System.Collections.Generic;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Abstractions;

public interface IExcludingRules
{
    bool IsExcluded(PrintingMemberData memberData);
    void Exclude<T>();
    void Exclude(Type type);
    void Exclude(IReadOnlyList<string> memberPath);
}