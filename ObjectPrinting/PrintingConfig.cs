using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private ImmutableDictionary<PropertyInfo, Func<object, string>> customPropertyRules; // I'm not sure how to force function to be given type
        private ImmutableDictionary<Type, Func<object, string>> customTypeRules; 
        private ImmutableDictionary<Type, Func<object, string>> defaultTypeRules; 

        private ImmutableHashSet<Type> excludedTypes;
        private ImmutableHashSet<PropertyInfo> excludedProperties;

        private HashSet<object> serializedObjects;

        public PrintingConfig()
        {
            customPropertyRules = ImmutableDictionary<PropertyInfo, Func<object, string>>.Empty;
            customTypeRules = ImmutableDictionary<Type, Func<object, string>>.Empty;
            defaultTypeRules = ImmutableDictionary<Type, Func<object, string>>.Empty;
            excludedTypes = ImmutableHashSet<Type>.Empty;
            excludedProperties = ImmutableHashSet<PropertyInfo>.Empty;
        }

        internal PrintingConfig(PrintingConfig<TOwner> currentConfig)
        {
            customPropertyRules = currentConfig.customPropertyRules;
            customTypeRules = currentConfig.customTypeRules;
            defaultTypeRules = currentConfig.defaultTypeRules;
            excludedTypes = currentConfig.excludedTypes;
            excludedProperties = currentConfig.excludedProperties;
        }

        internal PrintingConfig<TOwner> Minus(PropertyInfo propertyToExclude)
        {
            excludedProperties = excludedProperties.Add(propertyToExclude);
            return this;
        }

        internal PrintingConfig<TOwner> Minus(Type typeToExclude)
        {
            excludedTypes = excludedTypes.Add(typeToExclude);

            return this;
        }

        internal PrintingConfig<TOwner> WithDefaultTypeRule(Type type,
            Func<object, string> serialization)
        {
            defaultTypeRules = defaultTypeRules.Add(type, serialization);
            return this;
        }

        internal PrintingConfig<TOwner> WithCustomTypeRule(Type type,
            Func<object, string> serialization)
        {
            customTypeRules = customTypeRules.Add(type, serialization);
            return this;
        }

        internal PrintingConfig<TOwner> WithCustomPropertyRule(PropertyInfo propertyToUseCustomSerialization,
            Func<object, string> serialization)
        {
            customPropertyRules = customPropertyRules.Add(propertyToUseCustomSerialization, serialization);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            serializedObjects = new HashSet<object>();
            var objAsString = PrintToString(obj, 0);
            serializedObjects.Clear();
            return objAsString;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if(serializedObjects.Contains(obj))
                throw new FormatException("Found circular reference. Please specify custom serialization for circular reference types." +
                                          $"Found object: {obj}, with type {obj.GetType()}");

            serializedObjects.Add(obj);
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (IsEnumerableType(type))
            {
                var objAsEnumerable = (IEnumerable) obj;
                var elementNumber = 0;
                foreach (var enumerable in objAsEnumerable)
                {
                    sb.Append(identation + '\t' + elementNumber + " = " +
                              PrintToString(enumerable, nestingLevel + 1));
                    elementNumber++;
                }
            }
            else
            {
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (!excludedProperties.Contains(propertyInfo) &&
                        !excludedTypes.Contains(propertyInfo.PropertyType))
                    {
                        sb.Append(identation + propertyInfo.Name + " = " +
                                  GetStringRepresentation(propertyInfo, obj, nestingLevel));
                    }
                }
            }
            return sb.ToString();
        }

        private string GetStringRepresentation(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (customPropertyRules.ContainsKey(propertyInfo))
                return customPropertyRules[propertyInfo](propertyInfo.GetValue(obj));

            if (customTypeRules.ContainsKey(propertyInfo.PropertyType))
                return customTypeRules[propertyInfo.PropertyType](propertyInfo.GetValue(obj));

            if (defaultTypeRules.ContainsKey(propertyInfo.PropertyType))
                return defaultTypeRules[propertyInfo.PropertyType](propertyInfo.GetValue(obj));

            return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

        private bool IsEnumerableType(Type type)
        {
            return type.GetInterface(nameof(IEnumerable)) != null;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            return new PrintingConfig<TOwner>(this).Minus(typeof(T));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            var propInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            return new PrintingConfig<TOwner>(this).Minus(propInfo);
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            var propInfo = ((MemberExpression) property.Body).Member as PropertyInfo;
            return new PropertySerializationConfig<TOwner, TPropType>(this, propInfo);
        }
    }
}