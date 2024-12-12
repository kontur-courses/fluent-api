using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> simpleTypes =
        [
            typeof(bool), typeof(byte), typeof(int), typeof(double), typeof(float), typeof(char), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedProps = new();
        private readonly HashSet<object> alreadyProcessed = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<Type, CultureInfo> cultureForType = new();
        private readonly Dictionary<MemberInfo, CultureInfo> cultureForProp = new();
        private readonly Dictionary<MemberInfo, int> trimLengthForProp = new();
        private PropertyInfo? currentProp;

        protected internal readonly Dictionary<MemberInfo, Func<object, string>>
            AlternativeSerializationForProp = new();

        protected internal readonly Dictionary<Type, Func<object, string>> AlternativeSerializationForType = new();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        protected internal static MemberInfo GetPropDetails<T>(Expression<Func<TOwner, T>> expression)
        {
            return expression.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member,
                UnaryExpression { Operand: MemberExpression operand } => operand.Member,
                _ => throw new ArgumentException("Expression is not a property")
            };
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj == null)
                return string.Empty;
            if (alreadyProcessed.Contains(obj))
                return "[Recursion]";
            if (TryProcessSimpleType(obj, out var result))
                return result;
            if (TryProcessCollection(obj, nestingLevel, out result))
                return result;
            if (TryProcessDictionary(obj, nestingLevel, out result))
                return result;
            return ProcessNestedObject(obj, nestingLevel);
        }

        private bool TryProcessSimpleType(object obj, out string result)
        {
            var type = obj.GetType();
            if (simpleTypes.Contains(type))
            {
                result = (obj switch
                {
                    IFormattable f when currentProp != null && cultureForProp.TryGetValue(currentProp, out var culture) => f.ToString(null, culture),
                    IFormattable f when cultureForType.ContainsKey(type) => f.ToString(null, cultureForType[type]),
                    _ => obj.ToString()
                })!;

                if (currentProp != null && trimLengthForProp.TryGetValue(currentProp, out var length))
                    result = result.Length > length ? result[..length] : result;

                return true;
            }

            result = string.Empty;
            return false;
        }

        private bool TryProcessCollection(object obj, int nestingLevel, out string result)
        {
            if (obj is IEnumerable<object> collection)
            {
                var sb = new StringBuilder();
                foreach (var item in collection)
                    sb.Append(PrintToString(item, nestingLevel));
                result = sb.ToString();
                return true;
            }

            result = string.Empty;
            return false;
        }

        private bool TryProcessDictionary(object obj, int nestingLevel, out string result)
        {
            if (obj is IDictionary dictionary)
            {
                var sb = new StringBuilder();
                sb.Append('\t', nestingLevel);
                sb.AppendLine(obj.GetType().Name);
                foreach (DictionaryEntry entry in dictionary)
                {
                    var key = entry.Key;
                    var value = entry.Value;
                    sb.Append('\t', nestingLevel + 1);
                    sb.AppendLine($"{key.PrintToString()} = {value.PrintToString()}");
                }
                result = sb.ToString();
                return true;
            }

            result = string.Empty;
            return false;
        }

        private string ProcessNestedObject(object obj, int nestingLevel)
        {
            alreadyProcessed.Add(obj);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var property in type.GetProperties())
            {
                currentProp = property;
                if (excludedTypes.Contains(property.PropertyType) || excludedProps.Contains(property))
                    continue;
                var serializedValue = Serialize(property, obj, nestingLevel + 1);
                sb.Append('\t', nestingLevel + 1);
                sb.AppendLine($"{property.Name} = {serializedValue}");
            }
            currentProp = null;

            return sb.ToString();
        }

        private string Serialize(PropertyInfo property, object parent, int nestingLevel)
        {
            var value = property.GetValue(parent);
            if (value == null)
                return "null";
            if (AlternativeSerializationForProp.TryGetValue(property, out var propertySerializer))
                return propertySerializer(value);
            if (AlternativeSerializationForType.TryGetValue(property.PropertyType, out var typeSerializer))
                return typeSerializer(value);
            return PrintToString(value, nestingLevel);
        }

        public IPrintingConfig<TOwner> Exclude<TFieldType>()
        {
            excludedTypes.Add(typeof(TFieldType));
            return this;
        }

        public IPrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> expression)
        {
            excludedProps.Add(GetPropDetails(expression));
            return this;
        }

        public IMemberPrintingConfig<TOwner, TFieldType> Serialize<TFieldType>()
        {
            return new MemberPrintingConfig<TOwner, TFieldType>(this);
        }

        public IMemberPrintingConfig<TOwner, TFieldType> Serialize<TFieldType>(
            Expression<Func<TOwner, object>> expression)
        {
            return new MemberPrintingConfig<TOwner, TFieldType>(this, GetPropDetails(expression));
        }

        public IPrintingConfig<TOwner> SetCultureFor<TFieldType>(CultureInfo cultureInfo)
        {
            cultureForType.Add(typeof(TFieldType), cultureInfo);
            return this;
        }

        public IPrintingConfig<TOwner> SetCultureFor(Expression<Func<TOwner, object>> expression,
            CultureInfo cultureInfo)
        {
            cultureForProp.Add(GetPropDetails(expression), cultureInfo);
            return this;
        }

        public IPrintingConfig<TOwner> TrimString(Expression<Func<TOwner, string>> expression, int length)
        {
            trimLengthForProp.Add(GetPropDetails(expression), length);
            return this;
        }
    }
}