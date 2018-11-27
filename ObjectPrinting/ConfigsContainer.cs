using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class ConfigsContainer
    {
        public readonly HashSet<Type> TypesToExclude = new HashSet<Type>();
        public readonly HashSet<PropertyInfo> PropertiesToExclude = new HashSet<PropertyInfo>();

        public readonly Dictionary<Type, Func<object, string>> PrintersForTypes =
            new Dictionary<Type, Func<object, string>>();

        public readonly Dictionary<PropertyInfo, Func<object, string>> PrintersForProperties =
            new Dictionary<PropertyInfo, Func<object, string>>();
    }
}
