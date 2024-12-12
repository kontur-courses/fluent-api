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
        private readonly Dictionary<Type, Delegate> typeSerializers = new();
        private readonly Dictionary<MemberInfo, Delegate> propertySerializers = new();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new();
        private readonly Dictionary<MemberInfo, int> propertyTrim = new();
        private int maxNestingDepth = 10;

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyName = GetPropertyMemberInfo(propertySelector);
            excludedProperties.Add(propertyName);
            return this;
        }

        public PrintingConfig<TOwner> UseCulture<TPropType>(CultureInfo culture) where TPropType : IFormattable
        {
            typeCultures[typeof(TPropType)] = culture;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> PrintSettings<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberInfo = GetPropertyMemberInfo(propertySelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        public TypePrintingConfig<TOwner, TPropType> PrintSettings<TPropType>() =>
            new(this);

        public PrintingConfig<TOwner> SetDepth(int depth)
        {
            maxNestingDepth = depth;
            return this;
        }

        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 1);

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (typeSerializers.TryGetValue(type, out var serializer))
                return serializer.DynamicInvoke(obj) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;;

            if (obj is IEnumerable enumerable && type != typeof(string))
                return SerializeCollection(enumerable, nestingLevel);

            if (nestingLevel > maxNestingDepth)
                return $"Достигнут максимум глубины рекурсии - {maxNestingDepth}." + Environment.NewLine;

            var indentation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            sb.AppendLine($"{type.Name}");
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;

                if (propertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
                {
                    var formatValue = PrintToString(propertySerializer.DynamicInvoke(propertyValue), 0);
                    sb.Append($"{indentation}{propertyName} = {formatValue}");
                    continue;
                }

                if (typeCultures.TryGetValue(propertyType, out var culture))
                {
                    var formatValue = ((IFormattable)propertyValue).ToString(null, culture);
                    sb.Append($"{indentation}{propertyName} = {PrintToString(formatValue, 0)}");
                    continue;
                }

                if (propertyTrim.TryGetValue(propertyInfo, out var toTrimLength) && propertyValue is string stringValue)
                {
                    var formatValue = stringValue.Substring(0,
                        Math.Min(stringValue.Length - toTrimLength, stringValue.Length));
                    sb.Append($"{indentation}{propertyName} = {PrintToString(formatValue, 0)}");
                    continue;
                }

                sb.Append($"{indentation}{propertyName} = {PrintToString(propertyValue, nestingLevel + 1)}");
            }
            return sb.ToString();
        }

        private string SerializeCollection(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = GenerateIndentation(nestingLevel);

            if (collection is IDictionary dictionary)
            {
                sb.AppendLine("{");

                foreach (DictionaryEntry entry in dictionary)
                {
                    var key = entry.Key;
                    var value = entry.Value;

                    sb.Append($"{indentation}\"{key}\": {PrintToString(value, nestingLevel + 1)}");
                }

                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine("[");

                var items = (collection as IEnumerable<object>).ToArray();
                for (var i = 0; i < items.Length; i++)
                {
                    sb.Append($"{indentation}{PrintToString(items[i], nestingLevel + 1)}");
                }
                sb.AppendLine($"{GenerateIndentation(nestingLevel - 1)}]");
            }

            return sb.ToString();
        }

        private string GenerateIndentation(int nestingLevel)
        {
           return new string('\t', nestingLevel);
        }

        private MemberInfo GetPropertyMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpression)
                return memberExpression.Member;

            if (propertySelector.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
                return operand.Member;

            throw new ArgumentException("Invalid property selector expression");
        }

        internal void AddPropertySerializer<TPropType>(MemberInfo propertyInfo, Func<TPropType, string> serializer) =>
            propertySerializers[propertyInfo] = serializer;

        internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer) =>
            typeSerializers[typeof(TPropType)] = serializer;

        internal void AddStringPropertyTrim(MemberInfo propertyInfo, int maxLength) =>
            propertyTrim[propertyInfo] = maxLength;
    }
}