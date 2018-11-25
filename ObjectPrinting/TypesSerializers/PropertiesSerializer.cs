using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.TypesSerializers
{
    public class PropertiesSerializer : TypeSerializer
    {
        private readonly ImmutableHashSet<Type> excludedProperties;
        private readonly IReadOnlyDictionary<Type, Delegate> TypesSerializers;
        private readonly IReadOnlyDictionary<Type, CultureInfo> CustomCultures;
        private readonly IReadOnlyDictionary<PropertyInfo, Delegate> PropertiesSerializers;
        private readonly IReadOnlyDictionary<PropertyInfo, int> StringsTrimValues;
        private readonly ImmutableHashSet<PropertyInfo> excludedSpecificProperties;


        public PropertiesSerializer(
            ImmutableHashSet<Type> excludedProperties,
            IReadOnlyDictionary<Type, Delegate> typesSerializers,
            IReadOnlyDictionary<Type, CultureInfo> customCultures, 
            IReadOnlyDictionary<PropertyInfo, Delegate> propertiesSerializers,
            IReadOnlyDictionary<PropertyInfo, int> stringsTrimValues,
            ImmutableHashSet<PropertyInfo> excludedSpecificProperties)
        {
            this.excludedProperties = excludedProperties;
            TypesSerializers = typesSerializers;
            CustomCultures = customCultures;
            PropertiesSerializers = propertiesSerializers;
            StringsTrimValues = stringsTrimValues;
            this.excludedSpecificProperties = excludedSpecificProperties;
        }

        public override string Serialize(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues,
            TypeSerializer serializer)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var identation = new string('\t', nestingLevel + 1);

            foreach (var propertyInfo in type.GetProperties())
            {
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
                    sb.Append(identation + propertyInfo.Name + " = " + Constants.Circular);

                    continue;
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

                if (StringsTrimValues.TryGetValue(propertyInfo, out var count))
                {
                    var propertyValueAsString = propertyValue.ToString();
                    propertyValue = propertyValueAsString
                        .Substring(0, count);
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                    serializer.Serialize(propertyValue,
                        nestingLevel + 1, excludedValues.Add(obj), serializer));
            }

                return sb
                    .Append(Successor?.Serialize(obj, nestingLevel, excludedValues, serializer))
                    .ToString();
        }
    }
}