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
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> excludedTypes;
        private HashSet<PropertyInfo> exludedProperties;
        internal Dictionary<Type, Delegate> typePropertyConfigs;
        internal Dictionary<string, Delegate> namePropertyConfigs;
        private readonly int maxNestingLevel;

        public PrintingConfig(int maxNestingLevel = 10)
        {
            excludedTypes = new HashSet<Type>();
            exludedProperties = new HashSet<PropertyInfo>();
            typePropertyConfigs = new Dictionary<Type, Delegate>();
            namePropertyConfigs = new Dictionary<string, Delegate>();
            this.maxNestingLevel = maxNestingLevel;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression) memberSelector.Body).Member;
            var propInfo = member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Can't extract property from expression body");
            return new PropertyPrintingConfig<TOwner, TPropType>(this, member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            exludedProperties.Add(propInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> TrimmedToLength(int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentException("maxLength must be non-negative");
            return Printing<string>()
                .Using(s => maxLength > s.Length ? s : s.Substring(0, maxLength));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintCollectionToString(IEnumerable collection, int nestingLevel)
        {
            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel);

            var sb = new StringBuilder();
            sb.AppendLine();
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation);
            sb.AppendLine("[");
            foreach (var item in collection)
            {
                sb.Append(identation);
                sb.Append(PrintToString(item, nestingLevel + 1));
            }

            sb.Append(identation);
            sb.AppendLine("]");
            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation);
            sb.AppendLine("[");
            foreach (DictionaryEntry keyValuePair in dictionary)
            {
                var key = keyValuePair.Key;
                var value = keyValuePair.Value;
                sb.Append(identation);
                sb.Append(PrintToString(key, nestingLevel + 1));
                sb.Length -= Environment.NewLine.Length;
                sb.Append(" : ");
                sb.Append(PrintToString(value, 0));
            }

            sb.Append(identation);
            sb.AppendLine("]");
            return sb.ToString();
        }

        private string SerializeProperties(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var identation = new string('\t', nestingLevel + 1);
                if (exludedProperties.Contains(propertyInfo))
                    continue;
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                var value = propertyInfo.GetValue(obj);
                if (namePropertyConfigs.ContainsKey(propertyInfo.Name))
                    value = namePropertyConfigs[propertyInfo.Name].DynamicInvoke(value);
                else if (typePropertyConfigs.ContainsKey(propertyInfo.PropertyType))
                    value = typePropertyConfigs[propertyInfo.PropertyType].DynamicInvoke(value);
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(value,
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > maxNestingLevel)
                return "";
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
            {
                if (typePropertyConfigs.ContainsKey(objType))
                    return typePropertyConfigs[objType].DynamicInvoke(obj) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            if (obj is IEnumerable collection)
            {
                return PrintCollectionToString(collection, nestingLevel);
            }

            return SerializeProperties(obj, nestingLevel);
        }
    }
}