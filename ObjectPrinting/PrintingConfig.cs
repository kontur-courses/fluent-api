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
        public PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes = new HashSet<Type>(parent.excludedTypes);
            excludedProperties = new HashSet<PropertyInfo>(parent.excludedProperties);
            TypeSerializers = new TypeSerializerCollection(parent.TypeSerializers);
            PropertySerializers = new PropertySerializerCollection(parent.PropertySerializers);
        }

        public PrintingConfig()
        {
            TypeSerializers = new TypeSerializerCollection();
            PropertySerializers = new PropertySerializerCollection();
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
        }

        public string PrintObject(TOwner obj)
        {
            return PrintWithConfig(obj, 0);
        }

        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;

        public readonly TypeSerializerCollection TypeSerializers;

        public readonly PropertySerializerCollection PropertySerializers;

        public readonly Dictionary<Type, Func<string, string, string, string>> TypeFormatters =
            new Dictionary<Type, Func<string, string, string, string>>();

        public readonly Dictionary<PropertyInfo, Func<(string indent, string propertyName, string serializedProperty),
            string>> PropertyFormatters = new Dictionary<PropertyInfo, Func<(string indent, string propertyName,
            string serializedProperty), string>>();

        private int maxNestingLevel = 10;

        private string PrintWithConfig(object? obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            if (nestingLevel > maxNestingLevel)
            {
                return "...";
            }


            var indentation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(obj.GetType().Name);

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    stringBuilder.AppendLine(indentation + SerializeProperty(item, nestingLevel + 1));
                }
            }
            else
                PrintProperties(obj, nestingLevel, stringBuilder, indentation);

            return stringBuilder.ToString();
        }

        private void PrintProperties(object obj, int nestingLevel, StringBuilder stringBuilder,
            string indentation)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.GetValue(obj) == obj)
                {
                    stringBuilder.AppendLine($"{indentation}{propertyInfo.Name} = this");
                    continue;
                }

                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;

                var serializedProperty = SerializeProperty(propertyInfo.GetValue(obj), nestingLevel, propertyInfo);

                PrintProperty(stringBuilder, indentation, propertyInfo, serializedProperty);
            }
        }

        private void PrintProperty(StringBuilder stringBuilder, string indentation, PropertyInfo propertyInfo,
            string serializedProperty)
        {
            if (TypeFormatters.ContainsKey(propertyInfo.PropertyType))
            {
                var formatter = TypeFormatters[propertyInfo.PropertyType];
                stringBuilder.AppendLine(formatter(indentation, propertyInfo.Name, serializedProperty));
            }
            else if (PropertyFormatters.ContainsKey(propertyInfo))
            {
                var formatter = PropertyFormatters[propertyInfo];
                stringBuilder.AppendLine(formatter((indentation, propertyInfo.Name, serializedProperty)));
            }
            else
                stringBuilder.AppendLine($"{indentation}{propertyInfo.Name} = {serializedProperty}");
        }

        private string SerializeProperty(object obj, int nestingLevel, PropertyInfo? propertyInfo = null)
        {
            if (!(propertyInfo is null))
            {
                if (PropertySerializers.ContainsSerializerFor(propertyInfo))
                {
                    return PropertySerializers.GetSerializerFor(propertyInfo).Serialize(obj);
                }

                if (TypeSerializers.ContainsSerializerFor(propertyInfo.PropertyType))
                    return TypeSerializers.GetSerializerFor(propertyInfo.PropertyType).Serialize(obj);
            }

            return SerializeUsingDefaultSerializer(obj, nestingLevel);
        }

        private string SerializeUsingDefaultSerializer(object obj, int nestingLevel)
        {
            return obj.GetType().GetProperties().Length == 0
                ? obj.ToString()
                : PrintWithConfig(obj, nestingLevel + 1);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var childConfig = new PrintingConfig<TOwner>(this);
            childConfig.excludedTypes.Add(typeof(T));
            return childConfig;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var childConfig = new PrintingConfig<TOwner>(this);
            var propertyInfo = ((MemberExpression) func.Body)
                .Member as PropertyInfo;
            childConfig.excludedProperties.Add(propertyInfo);
            return childConfig;
        }

        public TypeSerializationConfig<TOwner, T> Serializing<T>()
        {
            return new TypeSerializationConfig<TOwner, T>(this);
        }

        public PropertySerializationConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> propertyProvider)
        {
            return new PropertySerializationConfig<TOwner, T>(this, propertyProvider);
        }

        public PropertySerializationConfig<TOwner, string> Serializing(
            Expression<Func<TOwner, string>> propertyProvider)
        {
            return new PropertySerializationConfig<TOwner, string>(this, propertyProvider);
        }

        public PropertySerializationConfig<TOwner, int> Serializing(Expression<Func<TOwner, int>> propertyProvider)
        {
            return new PropertySerializationConfig<TOwner, int>(this, propertyProvider);
        }

        public PrintingConfig<TOwner> UsingMaxRecursionLevel(int maxLevel)
        {
            return new PrintingConfig<TOwner>(this) {maxNestingLevel = maxLevel};
        }
    }
}