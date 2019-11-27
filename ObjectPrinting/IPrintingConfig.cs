using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> SerializationForType { get; }
        Dictionary<PropertyInfo, Func<object, string>> SerializationForProperty { get; }
        Dictionary<Type, CultureInfo> NumberTypesCultures { get; }
        Dictionary<PropertyInfo, int> MaxLengthOfStringProperty { get; }
    }
}