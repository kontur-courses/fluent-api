using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfigData
    {
        public readonly Dictionary<Type, Delegate> TypePrintingFunctions = new Dictionary<Type, Delegate>();
        public readonly Dictionary<Type, CultureInfo> NumberTypesToCulture = new Dictionary<Type, CultureInfo>();

        public readonly Dictionary<MemberInfo, Delegate> PropNamesPrintingFunctions =
            new Dictionary<MemberInfo, Delegate>();

        public readonly Dictionary<MemberInfo, int> StringPropNamesTrimming = new Dictionary<MemberInfo, int>();
        public readonly HashSet<MemberInfo> ExcludingPropertiesNames = new HashSet<MemberInfo>();
        public readonly HashSet<Type> ExcludingTypes = new HashSet<Type>();
        public readonly HashSet<object> VisitedProperties = new HashSet<object>();
    }
}