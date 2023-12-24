using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class Serializer
    {
        private readonly SerializationSettings settings;

        public Serializer(SerializationSettings serializationSettings)
        {
            settings = serializationSettings;
        }

        public string SerializeObject(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            if (TrySerializeCollection(obj, nestingLevel, out var collectionSerialization))
                return collectionSerialization;

            if (settings.GetSerializedObjects().Contains(obj))
                return type.Name;

            if (!type.IsValueType)
                settings.AddSerializedObject(obj);

            if (TrySerializeFinalType(obj, out var typeSerialization))
                return typeSerialization;

            var identation = new string('\t', nestingLevel + 1);
            var objectSerialization = new StringBuilder();
            objectSerialization.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!TrySerializeProperty(propertyInfo, obj, nestingLevel, out var propertySerialization))
                    continue;
                objectSerialization.Append(identation + propertyInfo.Name + " = " + propertySerialization +
                                           Environment.NewLine);
            }

            return objectSerialization.ToString();
        }

        private bool TrySerializeFinalType(object obj, out string serializedType)
        {
            var type = obj.GetType();
            if (settings.GetTypesSerializations().ContainsKey(type))
            {
                serializedType = settings.GetTypesSerializations()[type].Invoke(obj);

                return true;
            }

            if (settings.GetFinalTypes().Contains(type))
            {
                serializedType = obj.ToString();
                if (settings.GetTypesWithCulture().ContainsKey(obj.GetType()) && obj is IFormattable)
                    serializedType = ((IFormattable)obj).ToString(null, settings.GetTypesWithCulture()[type]);

                return true;
            }

            serializedType = "";

            return false;
        }

        private bool TrySerializeProperty(PropertyInfo propertyInfo, object obj, int nestingLevel,
            out string serializedProperty)
        {
            serializedProperty = "";
            if (settings.GetExcludingProperties().Contains(propertyInfo)
                || settings.GetExcludingTypes().Contains(propertyInfo.PropertyType))
                return false;

            var propertyMaxLength = settings.GetPropertiesToTrim().ContainsKey(propertyInfo)
                ? settings.GetPropertiesToTrim()[propertyInfo]
                : -1;

            serializedProperty = settings.GetPropertiesSerializations().ContainsKey(propertyInfo)
                ? settings.GetPropertiesSerializations()[propertyInfo].Invoke(propertyInfo.GetValue(obj))
                : SerializeObject(propertyInfo.GetValue(obj), nestingLevel + 1).TrimEnd();

            serializedProperty = propertyMaxLength >= 0
                ? serializedProperty[..settings.GetPropertiesToTrim()[propertyInfo]]
                : serializedProperty;

            return true;
        }

        private bool TrySerializeCollection(object obj, int nestingLevel, out string serializedCollection)
        {
            serializedCollection = "";
            var type = obj.GetType();
            if (!type.IsGenericType && !type.IsArray)
                return false;

            if (type.IsArray || type.GetGenericTypeDefinition() == typeof(List<>))
            {
                serializedCollection = SerializeEnumerable(obj, nestingLevel);

                return true;
            }

            if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                serializedCollection = SerializeDictionary(obj, nestingLevel);

                return true;
            }

            return false;
        }

        private string SerializeEnumerable(object obj, int nestingLevel)
        {
            var enumerable = (IEnumerable)obj;
            var serializedEnumerable = new StringBuilder();
            var identation = "\r\n" + new string('\t', nestingLevel + 1);

            foreach (var item in enumerable)
                serializedEnumerable.Append(identation + SerializeObject(item, nestingLevel));

            return serializedEnumerable.ToString();
        }

        private string SerializeDictionary(object obj, int nestingLevel)
        {
            var dict = (IDictionary)obj;
            var serializedDict = new StringBuilder();
            var identation = "\r\n" + new string('\t', nestingLevel + 1);

            foreach (var pair in dict)
            {
                var key = ((DictionaryEntry)pair).Key;
                var value = ((DictionaryEntry)pair).Value;
                serializedDict.Append(identation + SerializeObject(key, nestingLevel) + " : " +
                                      SerializeObject(value, nestingLevel));
            }

            return serializedDict.ToString();
        }
    }
}