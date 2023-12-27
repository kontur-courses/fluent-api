using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<PropertyInfo> excludedProperties = new List<PropertyInfo>();
        private readonly Dictionary<Type, Delegate> serializedByType = new Dictionary<Type, Delegate>();

        private readonly Dictionary<PropertyInfo, Delegate> serializedByPropertyInfo =
            new Dictionary<PropertyInfo, Delegate>();

        private readonly HashSet<object> printedObjects = new HashSet<object>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add( GetPropertyInfo(memberSelector));
            return this;
        }
        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        private static PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Expression '{memberSelector}' refers to a method, not a property.");

            if (memberExpression.Member is not PropertyInfo propInfo)
                throw new ArgumentException($"Expression '{memberSelector}' refers to a field, not a property.");
            
            return propInfo;
        }
        
        private static bool IsFinalType(Type type)
        {
            return type == typeof(string)
                   || type.IsPrimitive
                   || typeof(IFormattable).IsAssignableFrom(type);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (IsFinalType(type))
                return obj + Environment.NewLine;
            
            var sb = new StringBuilder();
            if (printedObjects.Contains(obj))
                return sb.AppendLine("(Cycle) " + type.FullName).ToString();

            sb.AppendLine(type.Name);
            if (IsArrayOrList(type))
            {
                return sb.Append(SerializeEnumerable(obj, nestingLevel)).ToString();
            }

            if (IsDictionary(type))
            {
                return sb.Append(SerializeEnumerable(obj, nestingLevel)).ToString();
            }

            printedObjects.Add(obj);

            sb.Append(PrintProperties(obj, nestingLevel, type));

            return sb.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel, Type type)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            foreach (var propertyInfo in type.GetProperties().Where(prop => !IsExcluded(prop)))
            {
                if (TrySerializeProperty(obj, propertyInfo, propertyInfo.PropertyType, out var serializedValue))
                {
                    sb.AppendLine($"{indentation}{propertyInfo.Name} = {serializedValue}");
                    continue;
                }

                if (IsArrayOrList(propertyInfo.PropertyType))
                {
                    sb.AppendLine(indentation + propertyInfo.Name + " = ");
                    sb.Append(SerializeEnumerable(propertyInfo.GetValue(obj), nestingLevel + 1));
                }
                else if (IsDictionary(propertyInfo.PropertyType))
                {
                    sb.AppendLine(indentation + propertyInfo.Name + " = ");
                    sb.Append(SerializeDictionary(propertyInfo.GetValue(obj), nestingLevel + 1));
                }
                else
                {
                    sb.Append(
                        $"{indentation}{propertyInfo.Name} = " +
                        $"{PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)}");
                }
            }

            return sb.ToString();
        }
        
        
        private bool TrySerializeProperty(object obj, PropertyInfo propertyInfo, Type propertyType,
            out string serializedValue)
        {
            serializedValue = null;
            Delegate valueToUse = null;
            if (serializedByPropertyInfo.TryGetValue(propertyInfo, out var propertyValue))
                valueToUse = propertyValue;
            if (serializedByType.TryGetValue(propertyType, out var typeValue))
                valueToUse = typeValue;

            return valueToUse != null && TrySerializeValue(valueToUse, propertyInfo.GetValue(obj), out serializedValue);
        }

        private static bool TrySerializeValue(Delegate serializer, object value, out string serializedValue)
        {
            try
            {
                serializedValue = serializer.DynamicInvoke(value)?.ToString();
                return true;
            }
            catch
            {
                serializedValue = null;
                return false;
            }
        }

        private static bool IsArrayOrList(Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }

        private static bool IsDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        private string SerializeEnumerable(object obj, int nestingLevel)
        {
            var enumerable = (IEnumerable)obj;
            var sb = new StringBuilder();
            if (enumerable == null) return "null" + Environment.NewLine;
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var value in enumerable)
            {
                sb.Append(indentation);
                sb.Append(PrintToString(value, nestingLevel));
                nestingLevel++;
            }

            return sb.ToString();
        }

        private string SerializeDictionary(object obj, int nestingLevel)
        {
            var dictionary = (IDictionary)obj;
            var sb = new StringBuilder();
            if (dictionary == null) return "null" + Environment.NewLine;
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var keyVal in dictionary)
            {
                sb.Append(indentation);
                var key = ((DictionaryEntry)keyVal).Key;
                var value = ((DictionaryEntry)keyVal).Value;
                sb.Append(PrintToString(key, nestingLevel) + " : " + PrintToString(value, nestingLevel));
            }

            return sb.ToString();
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return excludedProperties.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);
        }

        public void AddSerializeProperty<TPropType>(Func<TPropType, string> print, PropertyInfo propertyInfo)
        {
            serializedByPropertyInfo.Add(propertyInfo, print);
        }

        public void AddSerializeByType<TPropType>(Type type, Func<TPropType, string> print)
        {
            serializedByType.Add(type, print);
        }
    }
}