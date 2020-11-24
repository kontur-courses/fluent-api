using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class Config
    {
        public readonly List<Type> excludedTypes;
        public readonly List<PropertyInfo> exludedFields;
        public readonly Dictionary<PropertyInfo, Delegate> fieldSerializers;
        public readonly Dictionary<PropertyInfo, CultureInfo> numbersCulture;
        public readonly Dictionary<Type, Delegate> typesSerializer;

        public Config()
        {
            excludedTypes = new List<Type>();
            typesSerializer = new Dictionary<Type, Delegate>();
            numbersCulture = new Dictionary<PropertyInfo, CultureInfo>();
            exludedFields = new List<PropertyInfo>();
            fieldSerializers = new Dictionary<PropertyInfo, Delegate>();
        }
    }
}