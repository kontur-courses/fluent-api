using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Modules.PrintingConfig
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<string> excludedTypes;
        private readonly HashSet<string> excludedProperties;

        private readonly Dictionary<Type, Func<object, string>> typesSerializationRules;
        private readonly Dictionary<string, Func<object, string>> propertiesSerializationRules;

        private readonly Dictionary<Type, CultureInfo> formattableCultureRules;

        private readonly Dictionary<string, int> stringPropertiesTrimmingRules;
        private int stringTrimmingRule;

        private readonly HashSet<Type> finalTypes;

        private int maxNestingLevel;
        private readonly HashSet<object> nestingCollection;

        private int maxCollectionLength;

        private char indentationChar;
        private int indentationMultiplier;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<string>();
            excludedProperties = new HashSet<string>();
            typesSerializationRules = new Dictionary<Type, Func<object, string>>();
            propertiesSerializationRules = new Dictionary<string, Func<object, string>>();
            formattableCultureRules = new Dictionary<Type, CultureInfo>();
            stringPropertiesTrimmingRules = new Dictionary<string, int>();
            stringTrimmingRule = int.MaxValue;
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(long), typeof(DateTime), typeof(TimeSpan)
            };
            maxNestingLevel = 10;
            maxCollectionLength = 10;
            nestingCollection = new HashSet<object>();
            indentationChar = '\t';
            indentationMultiplier = 1;
        }

        public PrintingConfig<TOwner> WithNestingLevel(int maxNestingLevel)
        {
            this.maxNestingLevel = maxNestingLevel;
            return this;
        }

        public PrintingConfig<TOwner> WithCollectionLength(int maxCollectionLength)
        {
            this.maxCollectionLength = maxCollectionLength;
            return this;
        }

        public PrintingConfig<TOwner> WithIndentationChar(char indentationChar)
        {
            this.indentationChar = indentationChar;
            return this;
        }

        public PrintingConfig<TOwner> WithIdentationMultiplier(int multiplier)
        {
            indentationMultiplier = multiplier;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression))
                throw new InvalidOperationException($"{memberSelector} -> isn't member selector");
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

        internal void SetCultureForFormattable<TPropType>(CultureInfo culture)
            where TPropType : IFormattable
        {
            var type = typeof(TPropType);
            formattableCultureRules[type] = culture;
        }

        internal void SetTrimmingLength(string propertyName, int maxLength)
        {
            if (propertyName == null)
                stringTrimmingRule = maxLength;
            else
                stringPropertiesTrimmingRules[propertyName] = maxLength;
        }

        private static bool IsString(Type type)
        {
            return type == typeof(string);
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
            {
                var type = obj.GetType();
                result = obj.ToString();
                result = formattableCultureRules.ContainsKey(type)
                    ? ((IFormattable)obj).ToString(null, formattableCultureRules[type])
                    : obj.ToString();
            }
            if (nestingCollection.Contains(obj))
                result = "reached cycle";
            return result == null;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (!IsObjNeedsToBeParsed(obj, nestingLevel, out var result))
                return result;
            nestingCollection.Add(obj);
            var type = obj.GetType();
            var sb = new StringBuilder();

            sb.AppendLine(ParseName(type));

            if (obj is IEnumerable collection)
                sb.Append(ParseCollection(collection, nestingLevel));

            sb.Append(ParseProperties(obj, type, nestingLevel));
            return sb.ToString();
        }

        private string GetIndentation(int nestingLevel)
        {
            return new string(indentationChar, nestingLevel * indentationMultiplier);
        }

        private string ParseCollection(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var enumerator = collection.GetEnumerator();
            enumerator.MoveNext();
            var indentation = GetIndentation(nestingLevel + 1);
            sb.AppendLine($"{indentation}Items:");
            for (var i = 0; i < maxCollectionLength; i++)
            {
                var item = enumerator.Current;
                sb.AppendLine($"{indentation}{indentationChar}{item}");
                if (enumerator.MoveNext())
                    continue;
                sb.AppendLine("...");
                break;
            }
            return sb.ToString();
        }

        private string ParseProperties(object obj, Type type, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = GetIndentation(nestingLevel + 1);
            foreach (var property in GetNotExcludedProperties(type))
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;
                sb.Append($"{indentation}{property.Name} = ");
                sb.Append(ParseValue(property, property.GetValue(obj), nestingLevel));
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
    }
}