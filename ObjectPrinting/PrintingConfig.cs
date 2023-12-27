using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Configs;
using ObjectPrinting.Extensions;
using static ObjectPrinting.PrintingHelper;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> ignoredTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> ignoredProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<object, int> visited = new Dictionary<object, int>();

        private readonly Dictionary<Type, ITypePrintingConfig<TOwner>> typeConfigs =
            new Dictionary<Type, ITypePrintingConfig<TOwner>>();

        private readonly Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>> propertyConfigs =
            new Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>>();

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return NullString;

            var type = obj.GetType();
            if (FinalTypes.Contains(type))
            {
                return ApplyCulture(obj, type) + NewLine;
            }

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var indentation = new string('\t', nestingLevel + 1);
            if (visited.TryGetValue(obj, out var level) && nestingLevel - level > MaxCyclicDepth)
            {
                sb.Append($"{indentation}\"The limit of cyclic references\"{NewLine}");
                return sb.ToString();
            }

            if (type.IsClass)
                visited.TryAdd(obj, nestingLevel);

            if (obj is IEnumerable enumerable)
            {
                SerializeEnumerable(sb, enumerable, nestingLevel);
                return sb.ToString();
            }
            
            var properties = type.GetProperties()
                .IgnoreProperties(ignoredProperties)
                .IgnoreTypes(ignoredTypes);
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(obj);
                if (TrySerializeProperty(property, sb, indentation, propertyValue))
                    continue;

                if (TrySerializeType(property, sb, indentation, propertyValue))
                    continue;

                sb.Append(
                    $"{indentation}{property.Name} = {PrintToString(propertyValue, nestingLevel + 1)}");
            }

            return sb.ToString();
        }

        private bool TrySerializeProperty(PropertyInfo property, StringBuilder sb, string indentation,
            object propertyValue)
        {
            if (!propertyConfigs.TryGetValue(property, out var config) || config.Serializer is null) 
                return false;

            sb.Append(
                $"{indentation}{property.Name} = {config.Serializer(propertyValue)}{NewLine}");
                
            return true;
        }

        private bool TrySerializeType(PropertyInfo property, StringBuilder sb, string indentation,
            object propertyValue)
        {
            if (!typeConfigs.TryGetValue(property.PropertyType, out var config) || config.Serializer is null) 
                return false;
            
            sb.Append(
                $"{indentation}{property.Name} = {config.Serializer(propertyValue)}{NewLine}");

            return true;
        }

        private string ApplyCulture(object obj, Type objType)
        {
            if (!(obj is IConvertible convertible))
                return obj.ToString();

            if (!typeConfigs.TryGetValue(objType, out var config) || config.CultureInfo is null)
                return obj.ToString();

            return convertible.ToString(config.CultureInfo);
        }

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            ignoredTypes.Add(typeof(TProperty));

            return this;
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(
            Expression<Func<TOwner, TProperty>> propertySelector)
        {
            if (!(propertySelector.Body is MemberExpression expr))
                throw new ArgumentException(nameof(propertySelector));

            if (!(expr.Member is PropertyInfo property))
                throw new ArgumentException(nameof(propertySelector));

            var propertyConfig = new PropertyPrintingConfig<TOwner, TProperty>(this, property);
            propertyConfigs[property] = propertyConfig;

            return propertyConfig;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var type = typeof(TPropType);
            var typeConfig = new TypePrintingConfig<TOwner, TPropType>(this);
            typeConfigs[type] = typeConfig;

            return typeConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            if (!(propertySelector.Body is MemberExpression expr))
                throw new ArgumentException(nameof(propertySelector));

            if (!(expr.Member is PropertyInfo property))
                throw new ArgumentException(nameof(propertySelector));

            ignoredProperties.Add(property);

            return this;
        }

        private void SerializeEnumerable(StringBuilder sb, IEnumerable enumerable, int nestingLevel)
        {
            switch (enumerable)
            {
                case IDictionary dict:
                    SerializeDictionary(sb, dict, nestingLevel);
                    return;
                case null:
                    sb.Append(NullString);
                    return;
            }

            var indentation = new string('\t', nestingLevel + 1);
            sb.Append($"{indentation[..^1]}[{NewLine}");
            foreach (var obj2 in enumerable)
            {
                sb.Append(indentation + PrintToString(obj2, nestingLevel + 1));
            }

            sb.AppendLine($"{indentation[..^1]}]");
        }

        private void SerializeDictionary(StringBuilder sb, IDictionary dict, int nestingLevel)
        {
            if (dict is null)
            {
                sb.Append(NullString);
                return;
            }
            
            var indentation = new string('\t', nestingLevel + 1);
            sb.Append($"{indentation[..^1]}{{{NewLine}");
            foreach (var key in dict.Keys)
            {
                sb.Append($"{indentation}{key}: {PrintToString(dict[key], nestingLevel + 2)}");
            }

            sb.AppendLine($"{indentation[..^1]}}}");
        }
    }
}