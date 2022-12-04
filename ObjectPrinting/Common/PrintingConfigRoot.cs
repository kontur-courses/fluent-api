using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Common
{
    internal class PrintingConfigRoot
    {
        internal HashSet<Type> ExcludedTypes { get; set; } = new HashSet<Type>();
        internal Dictionary<Type, Func<object, string>> TypeSerializers { get; set; } = new Dictionary<Type, Func<object, string>>();
        internal Dictionary<Type, CultureInfo> TypeCulture { get; set; } = new Dictionary<Type, CultureInfo>();
        internal Dictionary<MemberInfo, Func<object, string>> PropertySerializers { get; } = new Dictionary<MemberInfo, Func<object, string>>();
        internal HashSet<MemberInfo> ExcludedProperties { get; } = new HashSet<MemberInfo>();
        internal Dictionary<MemberInfo, int> MaxStringPropertyLengths { get; } = new Dictionary<MemberInfo, int>();
    }
}