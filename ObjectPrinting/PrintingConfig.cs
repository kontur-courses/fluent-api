using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool), typeof(Guid)
        };
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly Dictionary<Type, Delegate> specialTypesSerializing = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, CultureInfo> typesCulture = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<string, Delegate> specialPropertiesSerializing = new Dictionary<string, Delegate>();
        internal int StringTrim = -1;

        void IPrintingConfig<TOwner>.AddSpecialTypeSerializing(Type type, Delegate format) => specialTypesSerializing[type] = format;
        void IPrintingConfig<TOwner>.AddTypeCulture(Type type, CultureInfo culture) => typesCulture[type] = culture;
        void IPrintingConfig<TOwner>.AddSpecialPropertySerializing(string propName, Delegate format) => specialPropertiesSerializing[propName] = format;

        private string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            ((MemberExpression)selector.Body).Member.Name;

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            excludedProperties.Add(GetPropertyName(selector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyName(selector));

        public string PrintToString(TOwner obj, int maxNestingForRecursiveFields = 10) =>
            PrintToString(obj, 0, maxNestingForRecursiveFields);

        private string PrintToString(object obj, int nestingLevel, int maxNestingForRecursiveFields)
        {
            if (nestingLevel > maxNestingForRecursiveFields)
                return string.Empty;

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            if (obj is IEnumerable enumerable)
            {
                PrintEnumerable(enumerable, sb, nestingLevel, maxNestingForRecursiveFields, indentation, type);
            }
            else
            {
                PrintUsualType(sb, nestingLevel, maxNestingForRecursiveFields, indentation, type, obj);
            }

            return sb.ToString();
        }

        private void PrintEnumerable(IEnumerable enumerable, StringBuilder sb, int nestingLevel,
            int maxNestingForRecursiveFields, string indentation, Type type)
        {
            sb.AppendLine(type.Name + " consists of");
            foreach (var element in enumerable)
            {
                sb.Append(indentation +
                          PrintToString(element,
                              nestingLevel + 1, maxNestingForRecursiveFields));
            }
        }

        private void PrintUsualType(StringBuilder sb, int nestingLevel,
            int maxNestingForRecursiveFields, string indentation, Type type, object obj)
        {
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in Filter(type.GetProperties()))
            {
                var propertyValue = GetConfigurationValue(propertyInfo, obj);

                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyValue,
                              nestingLevel + 1, maxNestingForRecursiveFields));
            }
        }

        private IEnumerable<PropertyInfo> Filter(IEnumerable<PropertyInfo> properties) =>
            properties
                .Where(property => !excludedTypes.Contains(property.PropertyType))
                .Where(property => !excludedProperties.Contains(property.Name));

        private object GetConfigurationValue(PropertyInfo propertyInfo, object obj)
        {
            var propertyValue = propertyInfo.GetValue(obj);
            if (specialTypesSerializing.TryGetValue(propertyInfo.PropertyType, out var @delegate))
                propertyValue = @delegate.DynamicInvoke(propertyInfo.GetValue(obj));

            if (typesCulture.TryGetValue(propertyInfo.PropertyType, out var cultureInfo))
                propertyValue = ((IFormattable)propertyInfo.GetValue(obj)).ToString(null, cultureInfo);

            if (specialPropertiesSerializing.TryGetValue(propertyInfo.Name, out @delegate))
                propertyValue = @delegate.DynamicInvoke(propertyInfo.GetValue(obj));

            if (propertyInfo.PropertyType == typeof(string) && StringTrim != -1)
                propertyValue = propertyInfo.GetValue(obj).ToString().SafetySubstring(0, StringTrim);

            return propertyValue;
        }
    }
}