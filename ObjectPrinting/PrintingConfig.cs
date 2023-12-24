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
        private readonly HashSet<PropertyInfo> excludedProperites = new HashSet<PropertyInfo>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        internal readonly Dictionary<Type, Func<object, string>> typeSerializers =
            new Dictionary<Type, Func<object, string>>();

        internal readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers =
            new Dictionary<PropertyInfo, Func<object, string>>();

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            excludedProperites.Add(propInfo);
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
            var sb = new StringBuilder();
            if (enumerable is IDictionary)
                return SerializeIDictionary((IDictionary)enumerable, nestedObjects);
            sb.Append("[ ");
            foreach (var element in enumerable)
            {
                sb.Append(PrintToString(element, nestedObjects).TrimEnd());
                sb.Append(" ");
            }

            sb.Append("]");
            return sb.ToString();
        }

        private string SerializeIDictionary(IDictionary dictionary, ImmutableHashSet<object> nestedObjects)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestedObjects.Count);
            sb.Append(identation + "{" + Environment.NewLine);
            nestedObjects = nestedObjects.Add(dictionary);
            foreach (DictionaryEntry dictionaryEntry in dictionary)
            {
                sb.Append(identation + "\t[" + Environment.NewLine);
                sb.Append("\t" + PrintToString(dictionaryEntry.Key, nestedObjects));
                sb.Append(identation + "\t:" + Environment.NewLine);
                sb.Append("\t" + PrintToString(dictionaryEntry.Value, nestedObjects));
                sb.Append(identation + "\t]," + Environment.NewLine);
            }

            sb.Append(identation + "}" + Environment.NewLine);
            return sb.ToString().TrimEnd();
        }

        private string PrintToString(object obj, ImmutableHashSet<object> nestedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
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
                if (Excluded(propertyInfo) || nestedObjects.Contains(propertyValue))
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

        private bool Excluded(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperites.Contains(propertyInfo);
        }
    }
}