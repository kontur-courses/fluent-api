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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly HashSet<string> excludedProperties = new HashSet<string>();

        private readonly Dictionary<Type, Delegate> typeSerializers = new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, Delegate> propertySerializers = new Dictionary<string, Delegate>();

        private readonly Dictionary<Type, CultureInfo> cultureSettings = new Dictionary<Type, CultureInfo>();

        private int stringTrimmingValue = -1;

        private readonly HashSet<object> currentPrintingObjects = new HashSet<object>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool), typeof(char),
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(uint), typeof(long), typeof(ulong), typeof(decimal),
            typeof(Guid)
        };

        private readonly Type[] typesWithoutCulture = { typeof(string), typeof(bool), typeof(char) };

        public string PrintToString(TOwner obj)
        {
            currentPrintingObjects.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (currentPrintingObjects.Contains(obj))
                return $"Cyclic reference detected{Environment.NewLine}";

            currentPrintingObjects.Add(obj);

            var type = obj.GetType();

            string result;
            if (IsFinalType(type))
            {
                result = HandleFinalType(obj, type);
            }
            else if (obj is IEnumerable collection)
            {
                result = HandleCollection(collection, nestingLevel, type);
            }
            else
            {
                result = HandleComplexObject(obj, nestingLevel, type);
            }

            currentPrintingObjects.Remove(obj);
            return result;
        }

        private bool IsFinalType(Type objType)
        {
            return finalTypes.Contains(objType);
        }

        private string HandleFinalType(object finalTypeObj, Type objType)
        {

            if (cultureSettings.ContainsKey(objType) && finalTypeObj is IFormattable formattable)
            {
                return formattable.ToString(null, cultureSettings[objType]) + Environment.NewLine;
            }

            return finalTypeObj + Environment.NewLine;
        }

        private string HandleCollection(IEnumerable collection, int nestingLevel, Type collectionType)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);

            if (collection is IDictionary dictionary)
            {
                sb.AppendLine("Dictionary");
                foreach (var key in dictionary.Keys)
                {
                    sb.Append(indentation + $"[{PrintSimpleValue(key)}] = ");
                    sb.Append(PrintToString(dictionary[key], nestingLevel + 1));
                }

                return sb.ToString();
            }
            else if (collectionType.IsArray)
            {
                sb.AppendLine("Array");
            }
            else if (collection is IList)
            {
                sb.AppendLine("List");
            }
            else
            {
                sb.AppendLine("Collection");
            }

            PrintCollection(sb, collection, indentation, nestingLevel);
            return sb.ToString();
        }

        private void PrintCollection(StringBuilder sb, IEnumerable collection, string indentation, int nestingLevel)
        {
            int index = 0;
            foreach (var item in collection)
            {
                sb.Append(indentation + $"[{index}] = ");
                sb.Append(PrintToString(item, nestingLevel + 1));
                index++;
            }
        }

        private string PrintSimpleValue(object value)
        {
            if (value == null) return "null";
            return value.ToString();
        }

        private string HandleComplexObject(object obj, int nestingLevel, Type objType)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);

            sb.AppendLine(objType.Name);
            foreach (var propertyInfo in objType.GetProperties())
            {
                if (ShouldSkipProperty(propertyInfo))
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
                sb.Append(indentation + propertyInfo.Name + " = ");
                sb.Append(HandlePropertyValue(propertyInfo, propertyValue, nestingLevel));
            }
            return sb.ToString();
        }

        private bool ShouldSkipProperty(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) ||
                   excludedProperties.Contains(propertyInfo.Name);
        }

        private string HandlePropertyValue(PropertyInfo propertyInfo, object propertyValue, int nestingLevel)
        {
            if (propertyInfo.PropertyType == typeof(string) && stringTrimmingValue > -1 && propertyValue != null)
            {
                var str = (string)propertyValue;
                propertyValue = str.Length <= stringTrimmingValue ? str : str.Substring(0, stringTrimmingValue);
            }

            if (typeSerializers.ContainsKey(propertyInfo.PropertyType))
            {
                var serializer = typeSerializers[propertyInfo.PropertyType];
                var serializedValue = serializer.DynamicInvoke(propertyValue);
                return PrintToString(serializedValue, nestingLevel + 1);
            }

            if (propertySerializers.ContainsKey(propertyInfo.Name))
            {
                var serializer = propertySerializers[propertyInfo.Name];
                var serializedValue = serializer.DynamicInvoke(propertyValue);
                return PrintToString(serializedValue, nestingLevel + 1);
            }

            return PrintToString(propertyValue, nestingLevel + 1);
        }

        public PrintingConfig<TOwner> Exclude<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpr)
            {
                excludedProperties.Add(memberExpr.Member.Name);
            }
            return this;
        }

        public PrintingConfig<TOwner> SetSerialization<TType>(Func<TType, string> func)
        {
            typeSerializers[typeof(TType)] = func;
            return this;
        }

        public PrintingConfig<TOwner> SetSerialization<TProperty>(
            Expression<Func<TOwner, TProperty>> propertySelector,
            Func<TProperty, string> serializer)
        {
            if (propertySelector.Body is MemberExpression member)
            {
                propertySerializers[member.Member.Name] = serializer;
            }
            return this;
        }

        public PrintingConfig<TOwner> SetCulture<T>(CultureInfo culture) where T : IFormattable
        {
            var type = typeof(T);

            if (typesWithoutCulture.Contains(type))
            {
                throw new ArgumentException($"Culture cannot be set for type {type.Name} because it doesn't support meaningful culture-specific formatting");
            }

            cultureSettings[type] = culture;
            return this;
        }

        public PrintingConfig<TOwner> TrimStringsTo(int length)
        {
            if (length < 0)
                throw new ArgumentException("Length must be non-negative");
            stringTrimmingValue = length;
            return this;
        }
    }
}