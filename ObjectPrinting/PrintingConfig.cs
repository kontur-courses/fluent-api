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
    public interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> AddCustomSerialization(Type type, Delegate func);
        PrintingConfig<TOwner> AddCustomPropertySerialization(string propertyName, Delegate func);
        PrintingConfig<TOwner> SetTypeCulture(Type type, CultureInfo culture);
        PrintingConfig<TOwner> Trim(int maxLen);
        PrintingConfig<TOwner> Trim(string propertyName, int maxLen);
    }

    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly TOwner value;

        private readonly Dictionary<Type, Delegate> customTypeSerializations = new Dictionary<Type, Delegate>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<string, Delegate> customPropertySerializations = new Dictionary<string, Delegate>();

        private readonly Dictionary<string, int> stringPropertiesToTrim = new Dictionary<string, int>();

        private readonly List<Type> excludedTypes = new List<Type>();

        private readonly List<string> excludedProperties = new List<string>();

        private bool trimEnabled;
        private int maxLen;
        private int maxNesting = 10;

        public PrintingConfig(TOwner value)
        {
            this.value = value;
        }

        public PrintingConfig()
        {
            value = default;
        }

        public string PrintToString()
        {
            return PrintToString(value);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            if (!excludedTypes.Contains(typeof(T)))
                excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> PrintingProperty<T>(Expression<Func<TOwner, T>> expression)
        {
            var propertyInfo = ((MemberExpression) expression.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, propertyInfo);
        }

        public PropertyPrintingConfig<TOwner, T> PrintingType<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            var propertyInfo = ((MemberExpression) expression.Body).Member as PropertyInfo;
            excludedProperties.Add(propertyInfo?.Name);
            return this;
        }

        public PrintingConfig<TOwner> WithMaximumNesting(int maxNesting)
        {
            this.maxNesting = maxNesting;
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            if (customTypeSerializations.ContainsKey(type))
            {
                var serializer = customTypeSerializations[type];
                return serializer.DynamicInvoke(obj) + Environment.NewLine;
            }

            var finalTypes = new[]
            {
                typeof(float), typeof(double), typeof(decimal),
                typeof(sbyte), typeof(byte),
                typeof(short), typeof(ushort),
                typeof(int), typeof(uint),
                typeof(long), typeof(ulong),
                typeof(string), typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(type))
            {
                if (!typeCultures.ContainsKey(type))
                    return obj.ToString() + Environment.NewLine;

                var culture = typeCultures[type];
                dynamic number = Convert.ChangeType(obj, type);
                return number.ToString(culture) + Environment.NewLine;
            }

            if (nestingLevel >= maxNesting)
                return type.Name + Environment.NewLine;

            if (obj is IEnumerable enumerable)
                return PrintEnumerable(enumerable, nestingLevel);

            return PrintProperties(obj, nestingLevel);
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)
                    || excludedProperties.Contains(propertyInfo.Name))
                    continue;

                var objValue = propertyInfo.GetValue(obj);

                if (propertyInfo.PropertyType == typeof(string))
                {
                    objValue = TrimToLengthIfNeeded(propertyInfo.Name, objValue as string);
                }

                sb.Append(indentation + propertyInfo.Name + " = ");
                if (customPropertySerializations.ContainsKey(propertyInfo.Name))
                    sb.Append(customPropertySerializations[propertyInfo.Name]
                                  .DynamicInvoke(objValue) + Environment.NewLine);
                else
                    sb.Append(PrintToString(objValue, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string PrintEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(enumerable.GetType().Name);
            var indentation = new string('\t', nestingLevel + 1);
            if (enumerable is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    sb.Append(indentation + entry.Key + " - " + PrintToString(entry.Value, nestingLevel + 1));
                }

                return sb.ToString();
            }


            foreach (var obj in enumerable)
            {
                sb.Append(indentation + " - " + PrintToString(obj, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string TrimToLengthIfNeeded(string propName, string str)
        {
            if (trimEnabled)
            {
                return str.Substring(0, maxLen);
            }

            if (stringPropertiesToTrim.ContainsKey(propName))
            {
                return str.Substring(0, stringPropertiesToTrim[propName]);
            }

            return str;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomSerialization(Type type, Delegate func)
        {
            customTypeSerializations[type] = func;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.SetTypeCulture(Type type, CultureInfo culture)
        {
            typeCultures[type] = culture;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPropertySerialization(string propertyName,
            Delegate func)
        {
            customPropertySerializations[propertyName] = func;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.Trim(int maxLen)
        {
            trimEnabled = true;
            this.maxLen = maxLen;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.Trim(string propertyName, int maxLen)
        {
            stringPropertiesToTrim[propertyName] = maxLen;
            return this;
        }
    }
}