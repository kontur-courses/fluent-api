using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> Serialised { get; }
        Dictionary<Type, Func<object, string>> CultureTypes { get; }
        Dictionary<string, Func<object, string>> SerialisedProperty { get; }
        Dictionary<string, Func<string, string>> Trimmer { get; }
    }
}