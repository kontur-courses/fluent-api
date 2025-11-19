using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    internal class ObjectSerializer
    {
        private readonly PrintingConfigData _config;
        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool), typeof(decimal),
            typeof(long), typeof(short), typeof(byte), typeof(char), typeof(Guid)
        };

        public ObjectSerializer(PrintingConfigData config)
        {
            _config = config;
        }

        public string Serialize(object obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        public string PrintToString(object obj, int nestingLevel, HashSet<object> visitedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (!visitedObjects.Add(obj))
                return $"Cyclic reference to {obj.GetType().Name}" + Environment.NewLine;

            try
            {
                var type = obj.GetType();

                if (FinalTypes.Contains(type))
                    return ApplySerialization(obj, type) + Environment.NewLine;

                if (CollectionSerializer.IsDictionary(type))
                    return CollectionSerializer.SerializeDictionary(obj, nestingLevel, visitedObjects, _config);

                if (CollectionSerializer.IsCollection(type))
                    return CollectionSerializer.SerializeCollection(obj, nestingLevel, visitedObjects, _config);

                return SerializeObject(obj, nestingLevel, visitedObjects);
            }
            finally
            {
                visitedObjects.Remove(obj);
            }
        }

        private string SerializeObject(object obj, int nestingLevel, HashSet<object> visitedObjects)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (_config.ExcludedTypes.Contains(propertyInfo.PropertyType) || 
                    _config.ExcludedProperties.Contains(propertyInfo.Name))
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
                var serializedValue = SerializeProperty(propertyInfo, propertyValue, nestingLevel, visitedObjects);

                sb.Append(identation + propertyInfo.Name + " = " + serializedValue);
            }

            return sb.ToString();
        }

        private string SerializeProperty(PropertyInfo propertyInfo, object propertyValue, int nestingLevel, HashSet<object> visitedObjects)
        {
            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.PropertyType;

            if (_config.PropertySerializers.TryGetValue(propertyName, out var propertySerializer))
                return propertySerializer(propertyValue) + Environment.NewLine;

            if (_config.PropertyCultures.TryGetValue(propertyName, out var culture) && propertyValue is IFormattable formattableProperty)
                return formattableProperty.ToString(null, culture) + Environment.NewLine;

            if (_config.TypeSerializers.TryGetValue(propertyType, out var typeSerializer))
                return typeSerializer(propertyValue) + Environment.NewLine;

            if (_config.TypeCultures.ContainsKey(propertyType) && propertyValue is IFormattable formattable)
                return formattable.ToString(null, _config.TypeCultures[propertyType]) + Environment.NewLine;

            return PrintToString(propertyValue, nestingLevel + 1, visitedObjects);
        }

        private string ApplySerialization(object value, Type type)
        {
            if (value == null)
                return "null";

            if (_config.TypeSerializers.TryGetValue(type, out var serializer))
                return serializer(value);

            if (_config.TypeCultures.ContainsKey(type) && value is IFormattable formattable)
                return formattable.ToString(null, _config.TypeCultures[type]);

            if (type == typeof(Guid))
                return value.ToString();

            return value.ToString() ?? "null";
        }
    }
}