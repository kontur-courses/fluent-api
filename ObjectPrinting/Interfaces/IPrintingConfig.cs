using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Delegate> TypeSerializers { get; }

        Dictionary<Type, CultureInfo> TypeCultureInfo { get; }

        Dictionary<PropertyInfo, Delegate> PropertySerializers { get; }

        Dictionary<PropertyInfo, int> PropertyTrimmers { get; }
    }
}
