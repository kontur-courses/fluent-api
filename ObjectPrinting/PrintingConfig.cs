using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedProperties = new HashSet<Type>();
        internal readonly IDictionary<Type, Delegate> TypesSerializers = new Dictionary<Type, Delegate>();
        internal readonly IDictionary<Type, CultureInfo> CustomCultures = new Dictionary<Type, CultureInfo>();
        internal readonly IDictionary<PropertyInfo, Delegate> PropertiesSerializers =
            new Dictionary<PropertyInfo, Delegate>();
        internal readonly IDictionary<PropertyInfo, int> StringsTrimValues = new Dictionary<PropertyInfo, int>();
        private readonly HashSet<PropertyInfo> excludedSpecificProperties = new HashSet<PropertyInfo>();

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedProperties.Add(typeof(TPropType));

            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> selector)
        {
            var member = (MemberExpression)selector.Body;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)member.Member);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            var member = (MemberExpression)selector.Body;
            excludedSpecificProperties.Add((PropertyInfo)member.Member);

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, ImmutableHashSet<object>.Empty);
        }

        private string PrintToString(object obj, int nestingLevel, ImmutableHashSet<object> excludedValues)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()))
            {
                return obj + Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsGenericType &&
                    typeof(ICollection<>).IsAssignableFrom(propertyInfo.PropertyType.GetGenericTypeDefinition()))
                {
                    sb.Append(identation + propertyInfo.Name + " = " + propertyInfo.PropertyType.Name);

                    continue;
                }

                var propertyValue = propertyInfo.GetValue(obj);

                if (excludedProperties.Contains(propertyInfo.PropertyType))
                {
                    continue;
                }

                if (excludedSpecificProperties.Contains(propertyInfo))
                {
                    continue;
                }

                if (excludedValues.Contains(propertyValue))
                {
                    throw new InfiniteRecursionException();
                }

                if (TypesSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
                {
                    propertyValue = typeSerializer.DynamicInvoke(propertyValue);
                }

                if (CustomCultures.TryGetValue(propertyInfo.PropertyType, out var culture))
                {
                    propertyValue = ((IConvertible)propertyValue).ToString(culture);
                }

                if (PropertiesSerializers.TryGetValue(propertyInfo, out var propSerializer))
                {
                    propertyValue = propSerializer.DynamicInvoke(propertyValue);
                }

                if (StringsTrimValues.TryGetValue(propertyInfo, out var startIndex))
                {
                    propertyValue = propertyValue.ToString()
                        .Substring(startIndex);
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                    PrintToString(propertyValue,
                        nestingLevel + 1, excludedValues.Add(propertyValue)));
            }

            return sb.ToString();
        }
    }
}