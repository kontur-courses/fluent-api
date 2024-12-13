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
        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers = new();
        private readonly Dictionary<PropertyInfo, int> propertyMaxLength = new();
        private readonly Dictionary<Type, CultureInfo> cultureForType = new();
        private readonly Dictionary<MemberInfo, CultureInfo> cultureForProp = new();
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<PropertyInfo> excludedProperties = new();
        private readonly HashSet<object> printed = new(ReferenceEqualityComparer.Instance);
        private PropertyInfo? currentProp;

        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public TypePrintingConfig<TOwner, TPropType> ForType<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> ForProperty<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public string PrintToString(TOwner? obj)
        {
            printed.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj == null)
                return string.Empty;
            if (IsRecursion(obj))
                return "(Recursion chain)";
            return obj switch
            {
                not null when IsFinalType(obj) => PrintFinalType(obj),
                IDictionary dict => PrintDictionary(dict, nestingLevel),
                IEnumerable list => PrintCollection(list, nestingLevel),
                _ => PrintComplexObject(obj, nestingLevel)
            };
        }

        private string PrintFinalType(object obj)
        {
            var type = obj.GetType();
            var result = (obj switch
            {
                IFormattable f when currentProp != null && cultureForProp.TryGetValue(currentProp, out var culture) =>
                    f.ToString(null, culture),
                IFormattable f when cultureForType.ContainsKey(type) => f.ToString(null, cultureForType[type]),
                _ => obj.ToString()
            })!;

            if (currentProp != null && propertyMaxLength.TryGetValue(currentProp, out var length))
                result = result.Length > length ? result[..length] : result;

            return result;
        }

        private bool IsRecursion(object obj)
        {
            if (printed.Contains(obj))
                return true;
            printed.Add(obj);
            return false;
        }

        private static bool IsFinalType(object obj) => FinalTypes.Contains(obj.GetType());

        private string PrintComplexObject(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var property in type.GetProperties())
            {
                currentProp = property;
                if (excludedTypes.Contains(property.PropertyType) || excludedProperties.Contains(property))
                    continue;
                var serializedValue = Serialize(property, obj, nestingLevel + 1);
                sb.Append('\t', nestingLevel + 1);
                sb.AppendLine($"{property.Name} = {serializedValue}");
            }

            currentProp = null;
            printed.Add(obj);
            return sb.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            foreach (var item in collection)
                sb.Append(PrintToString(item, nestingLevel));
            return sb.ToString();
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.Append('\t', nestingLevel);
            sb.AppendLine(dictionary.GetType().Name);
            foreach (DictionaryEntry entry in dictionary)
            {
                var key = entry.Key;
                var value = entry.Value;
                sb.Append('\t', nestingLevel + 1);
                sb.AppendLine($"{key.PrintToString()} = {value.PrintToString()}");
            }

            return sb.ToString();
        }

        private string Serialize(PropertyInfo property, object parent, int nestingLevel)
        {
            var value = property.GetValue(parent);
            if (value == null)
                return "null";
            
            if (propertySerializers.TryGetValue(property, out var propertySerializer))
                return propertySerializer(value);
            if (typeSerializers.TryGetValue(property.PropertyType, out var typeSerializer))
                return typeSerializer(value);

            return PrintToString(value, nestingLevel);
        }

        public void AddTypeSerializer<TPropType>(Func<object, string> print)
        {
            var type = typeof(TPropType);
            typeSerializers[type] = print;
        }

        public void AddPropertySerializer(PropertyInfo propertyInfo, Func<object, string?> print)
        {
            propertySerializers[propertyInfo] = print;
        }

        public void AddExcludedProperty(PropertyInfo propertyInfo)
        {
            excludedProperties.Add(propertyInfo);
        }

        public void AddExcludedType(Type type)
        {
            excludedTypes.Add(type);
        }

        public void AddTypeCulture<TType>(CultureInfo cultureInfo)
        {
            cultureForType.Add(typeof(TType), cultureInfo);
        }

        public void AddPropertyCulture(PropertyInfo propertyInfo, CultureInfo cultureInfo)
        {
            cultureForProp.Add(propertyInfo, cultureInfo);
        }

        public void AddPropertyMaxLenght(PropertyInfo propertyInfo, int maxLength)
        {
            propertyMaxLength.Add(propertyInfo, maxLength);
        }
    }
}