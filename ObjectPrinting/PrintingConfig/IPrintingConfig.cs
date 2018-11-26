using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> TypeSerializationSettings { get; }
        Dictionary<Type, CultureInfo> CultureInfoSettings { get; }
        Dictionary<(Type, string), Func<object, string>> PropertySerializationSettings { get; }
        Dictionary<(Type, string), int> PropertyTrimmingSettings { get; }
    }
}