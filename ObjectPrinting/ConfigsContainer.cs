using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class ConfigsContainer
    {
        public readonly HashSet<Type> TypesToExclude = new HashSet<Type>();
        public readonly HashSet<string> PropertiesToExclude = new HashSet<string>();

        public readonly Dictionary<Type, Func<object, string>> PrintersForTypes =
            new Dictionary<Type, Func<object, string>>();

        public readonly Dictionary<string, Func<object, string>> PrintersForProperties =
            new Dictionary<string, Func<object, string>>();
    }
}
