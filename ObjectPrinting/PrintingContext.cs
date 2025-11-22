using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingContext
{
    internal readonly HashSet<MemberInfo> excludedMembers = new();
    internal readonly HashSet<Type> excludedTypes = new();
    internal readonly Dictionary<MemberInfo, CultureInfo> memberCultures = new();
    internal readonly Dictionary<MemberInfo, Func<object, string>> memberSerializers = new();
    internal readonly Dictionary<Type, CultureInfo> typeCultures = new();
    internal readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
}