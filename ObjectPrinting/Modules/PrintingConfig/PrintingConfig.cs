using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ObjectPrinting.Modules.PrintingConfig
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<string> excludedTypes;
        private readonly HashSet<string> excludedProperties;

        private readonly Dictionary<Type, Func<object, string>> typesSerializationRules;
        private readonly Dictionary<string, Func<object, string>> propertiesSerializationRules;

        private readonly Dictionary<Type, CultureInfo> numbersCultureRules;

        private readonly Dictionary<string, int> stringPropertiesTrimmingRules;
        private int stringTrimmingRule;

        private readonly HashSet<Type> finalTypes;

        private readonly CultureInfo startCulture;

        private int maxNestingLevel;
        private readonly HashSet<object> nestingCollection;

        public PrintingConfig(int maxNestingLevel = 10)
        {
            excludedTypes = new HashSet<string>();
            excludedProperties = new HashSet<string>();
            typesSerializationRules = new Dictionary<Type, Func<object, string>>();
            propertiesSerializationRules = new Dictionary<string, Func<object, string>>();
            numbersCultureRules = new Dictionary<Type, CultureInfo>();
            stringPropertiesTrimmingRules = new Dictionary<string, int>();
            stringTrimmingRule = int.MaxValue;
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(long), typeof(DateTime), typeof(TimeSpan)
            };
            startCulture = CultureInfo.CurrentCulture;
            this.maxNestingLevel = maxNestingLevel;
            nestingCollection = new HashSet<object>();
        }

        public PrintingConfig<TOwner> WithNestingLevel(int maxNestingLevel)
        {
            this.maxNestingLevel = maxNestingLevel;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberExpression.Member.Name);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            excludedProperties.Add(memberExpression.Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType).Name);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            nestingCollection.Clear();
            return PrintToString(obj, 0);
        }

        internal void SetTypeSerialization<TPropType>(Func<TPropType, string> print)
        {
            var type = typeof(TPropType);
            typesSerializationRules[type] = obj => print((TPropType)obj);
        }

        internal void SetPropertySerialization<TPropType>(string propertyName, Func<TPropType, string> print)
        {
            propertiesSerializationRules[propertyName] = obj => print((TPropType)obj);
        }

        internal void SetTypeCulture<TPropType>(CultureInfo culture)
        {
            var type = typeof(TPropType);
            numbersCultureRules[type] = culture;
        }

        internal void SetTrimmingLength(string propertyName, int maxLength)
        {
            if (propertyName == null)
                stringTrimmingRule = maxLength;
            else
                stringPropertiesTrimmingRules[propertyName] = maxLength;
        }

        private static bool IsCultureDependent(Type type)
        {
            return type == typeof(double) || type == typeof(float);
        }

        private static bool IsString(Type type)
        {
            return type == typeof(string);
        }

        private void ChangeCulture(Type numberType)
        {
            Thread.CurrentThread.CurrentCulture = numbersCultureRules.ContainsKey(numberType)
                ? numbersCultureRules[numberType]
                : startCulture;
        }

        private void ResetCulture()
        {
            Thread.CurrentThread.CurrentCulture = startCulture;
        }

        private string TrimPropertyValue(string propertyValue, string propertyName)
        {
            var length = Math.Min(stringTrimmingRule, propertyValue.Length);
            if (stringPropertiesTrimmingRules.ContainsKey(propertyName))
                length = Math.Min(stringPropertiesTrimmingRules[propertyName], propertyValue.Length);

            return propertyValue.Substring(0, length);
        }

        private IEnumerable<PropertyInfo> GetNotExcludedProperties(Type type)
        {
            return type
                .GetProperties()
                .Where(prop =>
                    !excludedTypes.Contains(prop.PropertyType.Name)
                    && !excludedProperties.Contains(prop.Name));
        }

        private static string ParseName(Type type)
        {
            if (type.IsGenericType)
                return type.Name + string
                           .Concat(type.GenericTypeArguments
                           .Select(t => $"<{t.Name}>"));
            return type.Name;
        }

        private bool IsObjNeedsToBeParsed(object obj, int nestingLevel, out string result)
        {
            result = null;
            if (nestingLevel >= maxNestingLevel)
                result = "reached max nesting level";
            else if (obj == null)
                result = "null";
            else if (finalTypes.Contains(obj.GetType()))
                result = obj.ToString();
            if (nestingCollection.Contains(obj))
                result = "reached cycle";
            return result == null;
        }

        private string ParseProperties(object obj, Type type, string indentation, int nestingLevel)
        {
            var sb = new StringBuilder();
            foreach (var property in GetNotExcludedProperties(type))
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                if (IsCultureDependent(property.PropertyType))
                    ChangeCulture(property.PropertyType);

                sb.Append(indentation + property.Name + " = ");
                sb.Append(ParseValue(property, property.GetValue(obj), nestingLevel));

                ResetCulture();
            }

            return sb.ToString();
        }

        private string ParseValue(PropertyInfo property, object value, int nestingLevel)
        {
            string result;
            if (propertiesSerializationRules.ContainsKey(property.Name))
                result = propertiesSerializationRules[property.Name](value);
            else if (typesSerializationRules.ContainsKey(property.PropertyType))
                result = typesSerializationRules[property.PropertyType](value);
            else
                result = PrintToString(value, nestingLevel + 1);
            if (IsString(property.PropertyType))
                result = TrimPropertyValue(result, property.Name);
            if (!result.EndsWith(Environment.NewLine))
                result += Environment.NewLine;
            return result;
        }

        private static string ParseCollection(IEnumerable collection, string indentation)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{indentation}Items:");
            foreach (var item in collection)
                sb.AppendLine($"{indentation} \t\t {item}");
            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (!IsObjNeedsToBeParsed(obj, nestingLevel, out var result))
                return result;
            nestingCollection.Add(obj);
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(ParseName(type));

            if (obj is ICollection collection)
                sb.Append(ParseCollection(collection, indentation));

            sb.Append(ParseProperties(obj, type, indentation, nestingLevel));
            return sb.ToString();
        }
    }
}