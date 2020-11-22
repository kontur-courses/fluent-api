using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    internal class ConfigInfo
    {
        private readonly Dictionary<Type, CultureInfo> cultures;
        private readonly Dictionary<PropertyInfo, Func<object, string>> customPropertyPrinters;
        private readonly Dictionary<Type, Func<object, string>> customTypePrinters;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly HashSet<Type> excludedPropTypes;

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<PropertyInfo, int> maxLengths;

        internal ConfigInfo()
        {
            excludedPropTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            customTypePrinters = new Dictionary<Type, Func<object, string>>();
            customPropertyPrinters = new Dictionary<PropertyInfo, Func<object, string>>();
            cultures = new Dictionary<Type, CultureInfo>();
            maxLengths = new Dictionary<PropertyInfo, int>();
        }

        public bool IsFinal(Type type)
        {
            return finalTypes.Contains(type);
        }

        public bool IsExcluded(Type propType)
        {
            return excludedPropTypes.Contains(propType);
        }

        public bool IsExcluded(PropertyInfo propInfo)
        {
            return excludedProperties.Contains(propInfo);
        }

        public bool TryGetCustomPrinter(Type type, out Func<object, string> result)
        {
            return customTypePrinters.TryGetValue(type, out result);
        }

        public bool TryGetCustomPrinter(PropertyInfo propInfo, out Func<object, string> result)
        {
            return customPropertyPrinters.TryGetValue(propInfo, out result);
        }

        public bool TryGetCulture(Type type, out CultureInfo result)
        {
            return cultures.TryGetValue(type, out result);
        }

        public bool TryGetMaxLength(PropertyInfo propInfo, out int result)
        {
            return maxLengths.TryGetValue(propInfo, out result);
        }

        public void Exclude(Type type)
        {
            excludedPropTypes.Add(type);
        }

        public void Exclude(PropertyInfo propInfo)
        {
            excludedProperties.Add(propInfo);
        }

        public void RegisterCustomPrinter(Type type, Func<object, string> printer)
        {
            customTypePrinters[type] = printer;
        }

        public void RegisterCustomPrinter(PropertyInfo propInfo, Func<object, string> printer)
        {
            customPropertyPrinters[propInfo] = printer;
        }

        public void RegisterCulture(Type type, CultureInfo culture)
        {
            cultures[type] = culture;
        }

        public void SetMaxLength(PropertyInfo propInfo, int maxLength)
        {
            maxLengths[propInfo] = maxLength;
        }
    }
}