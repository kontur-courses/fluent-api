using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new();
        private readonly List<string> excludedProperties = new();
        private readonly Dictionary<Type, Func<object, string>> alternativeTypeSerializations = new();
        private readonly Dictionary<Type, CultureInfo> cultures = new();
        private readonly Dictionary<string, Func<object, string>> alternativePropertySerializations = new();
        
        public string PrintToString(TOwner obj)
        {
            var path = new List<object>();
            return PrintToString(obj, 0, path);
        }

        private string PrintToString(object obj, int nestingLevel, List<object> path)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (alternativeTypeSerializations.TryGetValue(obj.GetType(), out var alternativeTypeSerialization))
            {
                return alternativeTypeSerialization(obj) + Environment.NewLine;
            }

            if (cultures.TryGetValue(obj.GetType(), out var culture) && obj is IFormattable formatedProperty)
            {
                return formatedProperty.ToString(null, culture) + Environment.NewLine;
            }

            var valueType = obj.GetType();
            if (valueType.IsPrimitive
                || valueType.IsEnum
                || valueType == typeof(string)
                || valueType == typeof(decimal)
                || valueType == typeof(DateTime)
                || valueType == typeof(DateTimeOffset)
                || valueType == typeof(TimeSpan)
                || valueType == typeof(Guid))
            {
                return obj + Environment.NewLine;
            }

            foreach (var previous in path)
            {
                if (ReferenceEquals(previous, obj))
                    return "[cycle]" + Environment.NewLine;
            }

            path.Add(obj);

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (obj is IDictionary dictionary)
            {
                PrintDictionary(dictionary, nestingLevel, path, sb);
                path.RemoveAt(path.Count - 1);
                return sb.ToString();
            }

            if (obj is IEnumerable collection)
            {
                PrintCollection(collection, nestingLevel, path, sb);
                path.RemoveAt(path.Count - 1);
                return sb.ToString();
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                PrintMember(obj,
                    propertyInfo.Name,
                    propertyInfo.PropertyType,
                    () => propertyInfo.GetValue(obj),
                    nestingLevel, path, sb);
            }

            foreach (var field in type.GetFields())
            {
                PrintMember(obj,
                    field.Name,
                    field.FieldType,
                    () => field.GetValue(obj),
                    nestingLevel, path, sb);
            }
            path.RemoveAt(path.Count - 1);
            return sb.ToString();
        }

        private void PrintMember(object owner,
            string name,
            Type memberType,
            Func<object?> getValue,
            int nestingLevel, List<object> path, StringBuilder sb)
        {
            if (excludedTypes.Contains(memberType))
                return;
            if (owner is TOwner && excludedProperties.Contains(name))
                return;

            var value = getValue();
            var indentation = new string('\t', nestingLevel + 1);
            if (owner is TOwner &&
                alternativePropertySerializations.TryGetValue(name, out var serializer))
            {
                sb.AppendLine(indentation + name + " = " + serializer(value));
                return;
            }
            
            sb.Append(indentation + name + " = " +
                      PrintToString(value, nestingLevel + 1, path));
        }
        
        private void PrintCollection(IEnumerable collection,
            int nestingLevel, List<object> path, StringBuilder sb)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var index = 0;

            foreach (var item in collection)
            {
                sb.Append(indentation + "[" + index + "] = " +
                          PrintToString(item, nestingLevel + 1, path));

                index++;
            }
        }
        
        private void PrintDictionary(IDictionary dictionary,
            int nestingLevel, List<object> path, StringBuilder sb)
        {
            var indentation = new string('\t', nestingLevel + 1);

            foreach (DictionaryEntry entry in dictionary)
            {
                sb.Append(indentation + "Ключ: " +
                          PrintToString(entry.Key, nestingLevel + 1, path));

                sb.Append(indentation + "Значение: " +
                          PrintToString(entry.Value, nestingLevel + 1, path));
            }
        }
        
        public PrintingConfig<TOwner> Excluding(Type type)
        {
            excludedTypes.Add(type);
            return this;
        }

        public PrintingConfig<TOwner> Excluding(string property)
        {
            excludedProperties.Add(property);
            return this;
        }
        
        public PrintingConfig<TOwner> WithAlternativeSerialization<T>(Func<T, 
            string> alternativeSerialization)
        {
            alternativeTypeSerializations[typeof(T)] = 
                value => alternativeSerialization((T)value);
            return this;
        }
        
        public PrintingConfig<TOwner> WithAlternativeSerialization<TProperty>(string property,
            Func<TProperty, string> alternativeSerialization)
        {
            alternativePropertySerializations[property] = 
                value => alternativeSerialization((TProperty)value);
            return this;
        }
        
        public PrintingConfig<TOwner> WithCulture<T>(CultureInfo culture)
        {
            cultures[typeof(T)] = culture;
            return this;
        }
        
        internal PrintingConfig<TOwner> Trimmed(string propertyName, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            return WithAlternativeSerialization<string>(propertyName, 
                value => value == null
                    ? "null"
                    : value.Substring(0, Math.Min(value.Length, length)));
        }

        public PropertyConfig<TOwner, TValue> Printing<TValue>(
            Expression<Func<TOwner, TValue>> selector)
        {
            var member = (MemberExpression)selector.Body;
            return new PropertyConfig<TOwner, TValue>(
                this, member.Member.Name);
        }
    }
}
