using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, CultureInfo> Cultures { get; }
        Dictionary<Type, Delegate> TypePrintingConfig { get; }
        Dictionary<PropertyInfo, Delegate> PropertyToCut  { get; }
    }
}