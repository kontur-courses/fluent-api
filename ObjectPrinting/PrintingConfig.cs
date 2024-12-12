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
        private readonly List<Type> _excludedTypes = new();
        private readonly List<MemberInfo> _excludedProperties = new();
        private readonly Dictionary<Type, Delegate> _typeConverters = new();
        private readonly Dictionary<MemberInfo, Delegate> _propertyConverters = new();
        private readonly Dictionary<Type, CultureInfo> _cultureSpecs = new();
        private readonly Dictionary<MemberInfo, int> _stringPropertyLengths = new();
        internal int MaxStringLength { get; set; } = int.MaxValue;
        private int MaxRecursionDepth { get; set; } = 16;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, 0);
        }

        internal void AddCultureSpec(Type type, CultureInfo cultureInfo)
        {
            _cultureSpecs.Add(type, cultureInfo);
        }

        internal void AddStringPropertyLength(MemberInfo propertyInfo, int length)
        {
            _stringPropertyLengths.Add(propertyInfo, length);
        }

        public PrintingConfig<TOwner> WithMaxRecursionDepth(int maxRecursionDepth)
        {
            if (maxRecursionDepth < 0)
                throw new ArgumentOutOfRangeException($"{nameof(maxRecursionDepth)} must not be less than 0");
            MaxRecursionDepth = maxRecursionDepth;
            return this;
        }

        internal void AddTypeConverter<TParam>(Type type, Func<TParam, string?> converter)
        {
            _typeConverters.Add(type, converter);
        }

        internal void AddPropertyConverter<TParam>(Func<TParam, string> converter, MemberInfo propertyInfo)
        {
            _propertyConverters.Add(propertyInfo, converter);
        }

        public PrintingConfig<TOwner> ExceptType<T>()
        {
            _excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> ExceptProperty(Expression<Func<TOwner, object>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException($"{nameof(propertyExpression)} cannot be null");

            _excludedProperties.Add(GetMemberInfo(propertyExpression));
            return this;
        }

        public ITypeSerializer<TParam, TOwner> ForType<TParam>()
        {
            return new TypeSerializerImpl<TParam, TOwner>(this);
        }

        public IPropertySerializer<TOwner, TProperty> ForProperty<TProperty>(
            Expression<Func<TOwner, TProperty>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException($"{nameof(propertyExpression)} cannot be null");

            return new PropertySerializerImpl<TOwner, TProperty>(this, GetMemberInfo(propertyExpression));
        }

        private static MemberInfo GetMemberInfo<TProperty>(Expression<Func<TOwner, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
                return memberExpression.Member;
            
            if (propertyExpression.Body is UnaryExpression unaryExpression
                     && unaryExpression.Operand is MemberExpression unaryMemberExpression)
                return unaryMemberExpression.Member;
            
            throw new ArgumentException("Expression does not refer to a property or field.");
        }

        private string PrintToString(object obj, int nestingLevel, int recursionDepth)
        {
            if (obj == null)
                return "null";
            
            if (obj is IFormattable formattable
                && _cultureSpecs.TryGetValue(formattable.GetType(), out var cultureSpec))
                return $"{formattable.ToString(null, cultureSpec)}";
            
            if (obj is string str)
                return str.Substring(0, Math.Min(MaxStringLength, str.Length));
            
            if (obj.GetType().IsValueType)
                return $"{obj}";
            
            if (recursionDepth > MaxRecursionDepth)
                return "null";

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            
            if (obj is IDictionary dictionary)
                return SerializeDictionary(sb, dictionary, nestingLevel, recursionDepth);

            if (obj is IEnumerable enumerable)
                return SerializeEnumerable(sb, enumerable, nestingLevel, recursionDepth);
            
            var bracketIndentation = new string('\t', nestingLevel);
            sb.AppendLine($"{type.Name}:");
            sb.AppendLine($"{bracketIndentation}{{");
            var properties = type.GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (!_excludedProperties.Contains(propertyInfo) && !_excludedTypes.Contains(propertyInfo.PropertyType))
                {
                    var valueString = GetValueString(propertyInfo, obj, nestingLevel, recursionDepth);
                    sb.AppendLine($"{indentation}{propertyInfo.Name} = {valueString}");
                }
            }
            sb.Append($"{bracketIndentation}}}");
            return sb.ToString();
        }

        private string SerializeEnumerable(StringBuilder sb, IEnumerable enumerable, int nestingLevel, int recursionDepth)
        {
            var bracketIndentation = new string('\t', nestingLevel);
            sb.AppendLine($"[");
            if (!enumerable.GetEnumerator().MoveNext())
            {
                return "[]";
            }
            enumerable.GetEnumerator().Reset();
            foreach (var element in enumerable)
            {
                sb.Append($"{bracketIndentation}\t");
                var valueString = String.Empty;
                if (_typeConverters.TryGetValue(element.GetType(), out var typeConverter))
                    valueString =
                        $"{typeConverter.DynamicInvoke(element) as string ?? "null"}";
                else
                    valueString = PrintToString(element, nestingLevel + 1, recursionDepth + 1);
                sb.AppendLine($"{valueString},");
            }
            sb.Append($"{bracketIndentation}]");
            return sb.ToString();
        }

        private string SerializeDictionary(StringBuilder sb, IDictionary dictionary, int nestingLevel, int recursionDepth)
        {
            var bracketIndentation = new string('\t', nestingLevel);
            sb.AppendLine($"[");
            foreach (DictionaryEntry element in dictionary)
            {
                var key = element.Key;
                var value = element.Value;
                
                var keyValueIndentation = new string('\t', nestingLevel + 1);
                sb.AppendLine($"{bracketIndentation}{{");
                sb.Append($"{keyValueIndentation}key: ");
                var keyString = String.Empty;
                if (_typeConverters.TryGetValue(key.GetType(), out var typeConverter))
                    keyString =
                        $"{typeConverter.DynamicInvoke(key) as string ?? "null"}";
                else
                    keyString = PrintToString(key, nestingLevel + 2, recursionDepth + 1);
                sb.AppendLine($"{keyString}");
                sb.Append($"{keyValueIndentation}value: ");
                var valueString = String.Empty;
                if (_typeConverters.TryGetValue(value.GetType(), out typeConverter))
                    valueString =
                        $"{typeConverter.DynamicInvoke(value) as string ?? "null"}";
                else
                    valueString = PrintToString(value, nestingLevel + 2, recursionDepth + 1);
                sb.AppendLine($"{valueString}");
                sb.AppendLine($"{bracketIndentation}}},");
            }
            sb.AppendLine($"{bracketIndentation}]");
            return sb.ToString();
        }

        private string GetValueString(PropertyInfo propertyInfo, object obj, int nestingLevel, int recursionDepth)
        {
            var propertyValue = propertyInfo.GetValue(obj);
            if (propertyValue == null || !TryConvert(propertyInfo, propertyValue, out var valueString))
                valueString = PrintToString(propertyValue, nestingLevel + 1, recursionDepth + 1);
            return valueString;
        }

        private bool TryConvert(PropertyInfo propertyInfo, object? propertyValue, out string value)
        {
            value = String.Empty;
            if (_propertyConverters.TryGetValue(propertyInfo, out var converter))
                value = $"{converter.DynamicInvoke(propertyValue) as string ?? "null"}";
            else if (propertyValue is string str && _stringPropertyLengths.TryGetValue(propertyInfo, out var length))
                value = $"{str.Substring(0, Math.Min(length, str.Length))}";
            else if (_typeConverters.TryGetValue(propertyInfo.PropertyType, out var typeConverter))
                value = $"{typeConverter.DynamicInvoke(propertyValue) as string ?? "null"}";
            return value != String.Empty;
        }
    }
}