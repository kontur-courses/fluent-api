using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public abstract class Config
    {
        protected readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        protected readonly HashSet<PropertyInfo> excludingProperties = new HashSet<PropertyInfo>();

        protected internal readonly Dictionary<Type, Func<object, string>> typePrinters =
            new Dictionary<Type, Func<object, string>>();

        protected internal readonly Dictionary<Type, CultureInfo> typeCultureInfo =
            new Dictionary<Type, CultureInfo>();

        protected internal readonly Dictionary<PropertyInfo, int> trimmedProperties = 
            new Dictionary<PropertyInfo, int>();
    }
}