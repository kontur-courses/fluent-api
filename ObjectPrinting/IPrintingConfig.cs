using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<MemberInfo, Delegate> PropertyPrintingFunctions { get; }
        Dictionary<MemberInfo, int> PropertyStringsLength { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}