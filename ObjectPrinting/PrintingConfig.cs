using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        internal readonly Dictionary<Type, Func<object, string>> typeSerializers =
            new Dictionary<Type, Func<object, string>>();

        internal readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers =
            new Dictionary<PropertyInfo, Func<object, string>>();

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            excludedProperties.Add(propInfo);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, ImmutableHashSet<object>.Empty);
        }

        private string SerializeIEnumerable(IEnumerable enumerable, ImmutableHashSet<object> nestedObjects)
        {
            if (enumerable is IDictionary dictionary)
                return SerializeIDictionary(dictionary, nestedObjects);
            var enumerator = enumerable.GetEnumerator();
            var hasFirstElement = enumerator.MoveNext();
            if (!hasFirstElement)
                return "[]";
            var firstItemType = enumerator.Current;
            if (firstItemType.GetType().IsValueType || firstItemType is string)
            {
                return SerializeValueTypeIEnumerable(enumerable, nestedObjects);
            }

            return SerializeReferenceTypeIEnumerable(enumerable, nestedObjects);
        }

        private string SerializeValueTypeIEnumerable(IEnumerable enumerable, ImmutableHashSet<object> nestedObjects)
        {
            var sb = new List<string>();
            sb.Add("[");
            foreach (var element in enumerable)
            {
                var serializedString = PrintToString(element, nestedObjects).Trim();
                sb.Add(serializedString);
            }

            sb.Add("]");
            return string.Join(' ', sb);
        }

        private string SerializeReferenceTypeIEnumerable(IEnumerable enumerable, ImmutableHashSet<object> nestedObjects)
        {
            var sb = new List<string>();
            var identation = new string('\t', nestedObjects.Count);
            sb.Add(identation + "[");
            foreach (var element in enumerable)
            {
                var serializedString = PrintToString(element, nestedObjects.Add(sb)).TrimEnd();
                sb.Add(serializedString);
            }

            sb.Add(identation + "]");
            return string.Join(Environment.NewLine, sb);
        }

        private string SerializeIDictionary(IDictionary dictionary, ImmutableHashSet<object> nestedObjects)
        {
            var sb = new List<string>();
            var identation = new string('\t', nestedObjects.Count);
            sb.Add(identation + "{");
            nestedObjects = nestedObjects.Add(dictionary);
            foreach (DictionaryEntry dictionaryEntry in dictionary)
            {
                sb.Add(identation + '\t' + "[");
                sb.Add(identation + '\t' + PrintToString(dictionaryEntry.Key, nestedObjects).TrimEnd());
                sb.Add(identation + '\t' + ":");
                sb.Add(identation + '\t' + PrintToString(dictionaryEntry.Value, nestedObjects).TrimEnd());
                sb.Add(identation + '\t' + "]");
            }

            sb.Add(identation + "}");
            return string.Join(Environment.NewLine, sb);
        }

        private string PrintToString(object obj, ImmutableHashSet<object> nestedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objType = obj.GetType();
            if (obj.GetType().IsValueType || obj is string)
            {
                if (typeSerializers.TryGetValue(objType, out var serializer))
                    return serializer(obj) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            if (obj is IEnumerable enumerable)
                return SerializeIEnumerable(enumerable, nestedObjects) + Environment.NewLine;

            var identation = new string('\t', nestedObjects.Count + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objType.Name);
            nestedObjects = nestedObjects.Add(obj);
            foreach (var propertyInfo in objType.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);
                if (ExcludedFromSerialization(propertyInfo) || nestedObjects.Contains(propertyValue))
                    continue;

                if (propertySerializers.TryGetValue(propertyInfo, out var serializer))
                {
                    sb.Append(identation + propertyInfo.Name + " = " + serializer(propertyValue) + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestedObjects));
            }

            return sb.ToString();
        }

        private bool ExcludedFromSerialization(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo);
        }
    }
}