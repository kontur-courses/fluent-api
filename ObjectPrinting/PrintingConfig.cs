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
        public PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes = new HashSet<Type>(parent.excludedTypes);
            excludedProperties = new HashSet<PropertyInfo>(parent.excludedProperties);
            TypeSerializators = new TypeSerializerCollection(parent.TypeSerializators);
            PropertySerializators = new PropertySerializerCollection(parent.PropertySerializators);
        }

        public PrintingConfig()
        {
            TypeSerializators = new TypeSerializerCollection();
            PropertySerializators = new PropertySerializerCollection();
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
        }

        public string PrintObject(TOwner obj)
        {
            return PrintWithConfig(obj, 0);
        }

        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;

        public readonly TypeSerializerCollection TypeSerializators;

        public readonly PropertySerializerCollection PropertySerializators;

        public readonly Dictionary<Type, Func<string, string, string, string>> TypeFormatters =
            new Dictionary<Type, Func<string, string, string, string>>();

        public readonly Dictionary<PropertyInfo, Func<(string indent, string propertyName, string serializedProperty),
            string>> PropertyFormatters = new Dictionary<PropertyInfo, Func<(string indent, string propertyName,
            string serializedProperty), string>>();

        public int MaxNestingLevel = 10;

        private string PrintWithConfig(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (nestingLevel > MaxNestingLevel)
            {
                return "...";
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                if (!excludedTypes.Contains(propertyType))
                    if (!excludedProperties.Contains(propertyInfo))
                    {
                        string serializedProperty;
                        if (PropertySerializators.ContainsSerializerFor(propertyInfo))
                        {
                            serializedProperty = PropertySerializators.GetSerializerFor(propertyInfo)
                                .Serialize(propertyInfo.GetValue(obj));
                        }
                        else if (TypeSerializators.ContainsSerializerFor(propertyType))
                        {
                            serializedProperty =
                                TypeSerializators.GetSerializerFor(propertyType).Serialize(propertyInfo.GetValue(obj));
                        }
                        else
                        {
                            serializedProperty = PrintWithConfig(propertyInfo.GetValue(obj), nestingLevel + 1);
                        }

                        if (TypeFormatters.ContainsKey(propertyType))
                        {
                            var formatter = TypeFormatters[propertyType];
                            sb.Append(formatter(indentation, propertyInfo.Name, serializedProperty));
                        }
                        else if (PropertyFormatters.ContainsKey(propertyInfo))
                        {
                            var formatter = PropertyFormatters[propertyInfo];
                            sb.Append(formatter((indentation, propertyInfo.Name, serializedProperty)));
                        }
                        else
                            sb.Append(indentation + propertyInfo.Name + " = " + serializedProperty);
                    }
            }

            return sb.ToString();
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
            var childConfig = new PrintingConfig<TOwner>(this);
            childConfig.MaxNestingLevel = maxLevel;
            return childConfig;
        }
    }
}