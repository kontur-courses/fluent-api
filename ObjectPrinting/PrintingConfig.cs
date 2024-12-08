using System;
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
        private readonly Dictionary<string, Delegate> propertySerializers = new();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new();
        private readonly Dictionary<string, int> propertyTrim = new();

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyName = GetPropertyName(propertySelector);
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
            var memberInfo = GetPropertyName(propertySelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        public PropertyPrintingConfig<TOwner, TPropType> PrintSettings<TPropType>() =>
            new(this);

        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 1);

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (typeSerializers.TryGetValue(type, out var serializer))
                return serializer.DynamicInvoke(obj).ToString()!;

            if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
                return obj.ToString()!;

            var indentation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            sb.AppendLine($"{type.Name}");
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo))
                    continue;
                if(excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;

                if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
                {
                    sb.AppendLine($"{indentation}{propertySerializer.DynamicInvoke(propertyName)} = {propertyValue}");
                    continue;
                }

                if (typeCultures.TryGetValue(propertyType, out var culture))
                {
                    if (propertyValue is IFormattable formattable)
                    {
                        sb.AppendLine($"{indentation}{propertyName} = {formattable.ToString(null, culture)}");
                        continue;
                    }
                }

                if (propertyTrim.TryGetValue(propertyName, out var toTrimLength) && propertyValue is string stringValue)
                {
                    sb.AppendLine($"{indentation}{propertyName} = {stringValue.Substring(0, Math.Min(stringValue.Length - toTrimLength, stringValue.Length))}");
                    continue;
                }

                sb.AppendLine($"{indentation}{propertyName} = {PrintToString(propertyValue, nestingLevel + 1)}");
            }
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
    }
}