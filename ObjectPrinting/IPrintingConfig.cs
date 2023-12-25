using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public interface IPrintingConfig<TOwner>
{
    Dictionary<Type, Func<object, string>> TypeSerializers { get; }
    Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; }
    Dictionary<Type, CultureInfo> Cultures { get; }
    Dictionary<PropertyInfo, int> PropertiesToTrim { get; }
}
