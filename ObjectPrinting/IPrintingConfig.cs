using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> Serialised { get; }
        CultureInfo CultureType { get; set; }
        Dictionary<string, Func<object, string>> SerialisedProperty { get; }
        Dictionary<string, Func<string, string>> Trimmer { get; }
        Func<string, string> Trim { get; set; }
    }
}