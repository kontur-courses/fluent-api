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
            TypeSerializators = new Dictionary<Type, TypeSerializer>(parent.TypeSerializators);
            PropertySerializators = new Dictionary<PropertyInfo, PropertySerializer>(parent.PropertySerializators);
        }

        public PrintingConfig()
        {
        }

        public string PrintWithConfig(TOwner obj)
        {
            return PrintWithConfig(obj, 0);
        }

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        public readonly Dictionary<Type, TypeSerializer> TypeSerializators =
            new Dictionary<Type, TypeSerializer>();

        public readonly Dictionary<PropertyInfo, PropertySerializer> PropertySerializators =
            new Dictionary<PropertyInfo, PropertySerializer>();

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

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!excludedTypes.Contains(propertyInfo.PropertyType))
                    if (!excludedProperties.Contains(propertyInfo))
                    {
                        string serializedProperty;
                        if (PropertySerializators.ContainsKey(propertyInfo))
                        {
                            serializedProperty = PropertySerializators[propertyInfo].Serialize(propertyInfo.GetValue(obj));
                        }
                        else if (TypeSerializators.ContainsKey(propertyInfo.PropertyType))
                        {
                            serializedProperty =
                                TypeSerializators[propertyInfo.PropertyType].Serialize(propertyInfo.GetValue(obj));
                        }
                        else
                        {
                            serializedProperty = PrintWithConfig(propertyInfo.GetValue(obj), nestingLevel + 1);
                        }

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
    }
}