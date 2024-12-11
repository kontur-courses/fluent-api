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
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedProperties = new();
        private readonly Dictionary<Type, Delegate> typeSerializers = new(); // конкретный делегат
        private readonly Dictionary<string, Delegate> propertySerializers = new(); // конкретный делегат
        private readonly Dictionary<Type, CultureInfo> typeCultures = new();
        private readonly Dictionary<string, int> propertyTrim = new();
        private int MaxNestingLevel = 5;

        private readonly HashSet<Type> finalTypes =
        [
            typeof(bool), typeof(sbyte),  typeof(byte),  typeof(short),  typeof(ushort),
            typeof(int),  typeof(uint),  typeof(long), typeof(ulong), typeof(float),
            typeof(double),  typeof(decimal),  typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];

        // 1. Исключить из сериализации свойства определенного типа
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        // 2.  Указать альтернативный способ сериализации для определенного типа
        public PrintingConfiguration<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberInfo = GetPropertyName(propertySelector);
            return new PrintingConfiguration<TOwner, TPropType>(this, memberInfo);
        }

        public PrintingConfiguration<TOwner, TPropType> Printing<TPropType>()
        {
            return new PrintingConfiguration<TOwner, TPropType>(this, null);
        }

        // 6. Исключить из сериализации конкретного свойства
        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyName = GetPropertyName(propertySelector);
            excludedProperties.Add(propertyName);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > MaxNestingLevel)
                return "Достигнут максимум глубины сериализации";

            if (obj == null)
                return "null";

            var type = obj.GetType();

            if (excludedTypes.Contains(type))
                return string.Empty;

            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();

            if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
                return formattable.ToString(null, culture);

            if (typeSerializers.TryGetValue(type, out var serializer))
                return serializer.DynamicInvoke(obj).ToString();

            if (obj is ICollection collection)
                return SerializeCollection(collection, nestingLevel);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;

                if (excludedProperties.Contains(propertyInfo))
                    continue;
                if (excludedTypes.Contains(propertyType))
                    continue;

                if (typeSerializers.TryGetValue(propertyType, out var typeSerializer))
                {
                    sb.AppendLine($"{identation}{propertyName} = {typeSerializer.DynamicInvoke(propertyValue)}");
                    continue;
                }

                if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
                {
                    sb.AppendLine($"{identation}{propertyName} = {propertySerializer.DynamicInvoke(propertyValue)}");
                    continue;
                }

                if (typeCultures.TryGetValue(propertyType, out var cult))
                {
                    if (propertyValue is IFormattable format)
                    {
                        sb.AppendLine($"{identation}{propertyName} = {format.ToString(null, cult)}");
                        continue;
                    }
                }

                if (propertyTrim.TryGetValue(propertyName, out var toTrimLength) && propertyValue is string stringValue)
                {
                    sb.AppendLine($"{identation}{propertyName} = {stringValue.Substring(0, Math.Min(toTrimLength, stringValue.Length))}");
                    continue;
                }

                sb.AppendLine($"{identation}{propertyName} = {PrintToString(propertyValue, nestingLevel + 1)}");
            }
            return sb.ToString();
        }

        private string SerializeCollection(ICollection collection, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine("[");

            if (collection is IDictionary dictionary)
            {
                foreach (var key in dictionary.Keys)
                {
                    sb.Append($"{identation}{{{PrintToString(key, nestingLevel)}: " +
                              $"{PrintToString(dictionary[key], nestingLevel + 1)}}}");

                    //sb.Append($"{identation}{{{PrintToString(key, nestingLevel)} = ");
                    //sb.Append(PrintToString(dictionary[key], nestingLevel + 1) + "}; ");
                }
            }
            else
            {
                foreach (var value in collection)
                {
                    sb.Append('\t', nestingLevel + 1);
                    sb.Append(PrintToString(value, nestingLevel + 1));
                }
            }

            sb.AppendLine($"{identation}]");
            return sb.ToString();
        }

        private MemberInfo GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpression)
                return memberExpression.Member;

            if (propertySelector.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
                return operand.Member;

            throw new ArgumentException("Invalid property selector expression");
        }

        internal void AddPropertySerializer<TPropType>(string propertyName, Func<TPropType, string> serializer) =>
            propertySerializers[propertyName] = serializer;

        internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer) =>
            typeSerializers[typeof(TPropType)] = serializer;

        internal void AddStringPropertyTrim(string propertyName, int maxLength) =>
            propertyTrim[propertyName] = maxLength;

        internal void AddNumericCulture<TNumericCulture>(CultureInfo culture) =>
            typeCultures[typeof(TNumericCulture)] = culture;
    }
}