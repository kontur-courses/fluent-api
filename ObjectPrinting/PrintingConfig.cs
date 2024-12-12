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
        private MemberInfo? currentProp;

        protected internal readonly Dictionary<MemberInfo, Func<object, string>>
            AlternativeSerializationForProp = new();

        protected internal readonly Dictionary<Type, Func<object, string>> AlternativeSerializationForType = new();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private static MemberInfo GetPropDetails<T>(Expression<Func<TOwner, T>> expression)
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
            return obj switch
            {
                not null when simpleTypes.Contains(obj.GetType()) => ProcessSimpleType(obj),
                IDictionary dict => ProcessDictionary(dict, nestingLevel),
                IEnumerable list => ProcessCollection(list, nestingLevel),
                _ => ProcessNestedObject(obj, nestingLevel)
            };
        }

        private string ProcessSimpleType(object obj)
        {
            var type = obj.GetType();
            var result = (obj switch
            {
                IFormattable f when currentProp != null && cultureForProp.TryGetValue(currentProp, out var culture) =>
                    f.ToString(null, culture),
                IFormattable f when cultureForType.ContainsKey(type) => f.ToString(null, cultureForType[type]),
                _ => obj.ToString()
            })!;

            if (currentProp != null && trimLengthForProp.TryGetValue(currentProp, out var length))
                result = result.Length > length ? result[..length] : result;

            return result;
        }

        private string ProcessCollection(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            foreach (var item in collection)
                sb.Append(PrintToString(item, nestingLevel));
            return sb.ToString();
        }

        private string ProcessDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.Append('\t', nestingLevel);
            sb.AppendLine(dictionary.GetType().Name);
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                sb.Append('\t', nestingLevel + 1);
                sb.AppendLine($"{key.PrintToString()} = {value.PrintToString()}");
            }

            return sb.ToString();
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

            foreach (var field in type.GetFields())
            {
                currentProp = field;
                if (excludedTypes.Contains(field.FieldType) || excludedProps.Contains(field))
                    continue;
                var serializedValue = Serialize(field, obj, nestingLevel + 1);
                sb.Append('\t', nestingLevel + 1);
                sb.AppendLine($"{field.Name} = {serializedValue}");
            }

            currentProp = null;

            return sb.ToString();
        }

        private string Serialize(MemberInfo property, object parent, int nestingLevel)
        {
            object value;
            Type type;
            switch (property)
            {
                case PropertyInfo prop:
                    value = prop.GetValue(parent);
                    type = prop.PropertyType;
                    break;
                case FieldInfo field:
                    value = field.GetValue(parent);
                    type = field.FieldType;
                    break;
                default:
                    return string.Empty;
            }
            if (value == null)
                return "null";
            if (AlternativeSerializationForProp.TryGetValue(property, out var propertySerializer))
                return propertySerializer(value);
            if (AlternativeSerializationForType.TryGetValue(type, out var typeSerializer))
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