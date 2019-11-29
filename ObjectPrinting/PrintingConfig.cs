using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Utils;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private ImmutableDictionary<PropertyInfo, Func<object, string>> customPropertyRules;
        private ImmutableDictionary<Type, Func<object, string>> customTypeRules; 
        private ImmutableDictionary<Type, Func<object, string>> defaultTypeRules; 

        private ImmutableHashSet<Type> excludedTypes;
        private ImmutableHashSet<PropertyInfo> excludedProperties;

        private HashStack<object> serializingObjects;  

        private static readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private static readonly string NullRepresentation = "null" + Environment.NewLine;

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

        internal string GetStringRepresentation(TOwner obj)
        {
            serializingObjects = new HashStack<object>();
            var objAsString = GetStringRepresentation(obj, 0);
            serializingObjects.Clear();
            return objAsString;
        }

        private string GetStringRepresentation(object obj, int nestingLevel)
        {
            if (obj == null)
                return NullRepresentation;

            if (serializingObjects.Contains(obj))
                throw new FormatException("Found circular reference. Please specify custom serialization for circular reference types." +
                                          $"Found object: {obj}, with type {obj.GetType()}");
            serializingObjects.Push(obj);

            if (TryGetPrimitiveTypesAsString(obj, out var primitiveAsString))
                return primitiveAsString;

            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            sb.Append(IsEnumerableType(type)
                ? GetCollectionAsString(obj as IEnumerable, nestingLevel)
                : GetClassAsString(type, obj, nestingLevel));

            serializingObjects.Pop();
            return sb.ToString();
        }

        private string GetCollectionAsString(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var elementNumber = 0;
            foreach (var enumerable in collection)
            {
                sb.Append(identation + '\t' + elementNumber + " = " +
                          GetStringRepresentation(enumerable, nestingLevel + 1));
                elementNumber++;
            }
            return sb.ToString();
        }

        private string GetClassAsString(Type type, object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!excludedProperties.Contains(propertyInfo) &&
                    !excludedTypes.Contains(propertyInfo.PropertyType))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              GetPropertyAsString(propertyInfo, obj, nestingLevel));
                }
            }
            return sb.ToString();
        }

        private bool TryGetPrimitiveTypesAsString(object obj, out string valueAsString)
        {
            var type = obj.GetType();
            if (finalTypes.Contains(type))
            {
                if (customTypeRules.ContainsKey(type))
                    valueAsString = customTypeRules[type](obj) + Environment.NewLine;
                else if (defaultTypeRules.ContainsKey(type))
                    valueAsString = defaultTypeRules[type](obj) + Environment.NewLine;
                else
                    valueAsString = obj + Environment.NewLine;
                return true;
            }
            valueAsString = null;
            return false;
        }

        private string GetPropertyAsString(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var value = propertyInfo.GetValue(obj);

            if (value == null)
                return NullRepresentation;

            if (customPropertyRules.ContainsKey(propertyInfo))
                return customPropertyRules[propertyInfo](value);

            if (customTypeRules.ContainsKey(propertyInfo.PropertyType))
                return customTypeRules[propertyInfo.PropertyType](value);

            if (defaultTypeRules.ContainsKey(propertyInfo.PropertyType))
                return defaultTypeRules[propertyInfo.PropertyType](value);

            return GetStringRepresentation(value, nestingLevel + 1);
        }

        private bool IsEnumerableType(Type type)
        {
            return type.GetInterface(nameof(IEnumerable)) != null;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes = excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            var propInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            excludedProperties = excludedProperties.Add(propInfo);
            return this;
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