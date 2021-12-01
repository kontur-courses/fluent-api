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
        private readonly HashSet<PropertyInfo> excludedProperties = new();
        private readonly HashSet<Type> excludedTypes = new();
        public readonly Dictionary<Type, Delegate> TypeToPrinting = new();
        public readonly Dictionary<Type, CultureInfo> TypeToCultureInfo = new();
        public readonly Dictionary<PropertyInfo, Delegate> PropertyToPrinting = new();
        public readonly Dictionary<PropertyInfo, int> StringPropertyToLength = new();
        private readonly HashSet<object> printedObjects = new();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(long), typeof(bool), typeof(Guid)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(((MemberExpression)memberSelector.Body).Member as PropertyInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            return GetRepresentation(obj, nestingLevel);
        }

        private string GetRepresentation(object obj, int nestingLevel)
        {
            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                return GetFinalTypeRepresentation(obj);
            }

            if (printedObjects.Contains(obj))
                return "!already printed!";
            printedObjects.Add(obj);

            if (obj is IEnumerable enumerable)
            {
                return GetEnumerableRepresentation(enumerable, nestingLevel);
            }

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo)
                    || excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                sb.Append(GetPropertyRepresentation(obj, nestingLevel, propertyInfo));
            }

            return sb.ToString();
        }

        private string GetPropertyRepresentation(object obj, int nestingLevel, PropertyInfo propertyInfo)
        {
            var propertyValue = propertyInfo.GetValue(obj);
            if (StringPropertyToLength.TryGetValue(propertyInfo, out var length))
            {
                propertyValue = ((string)propertyValue)?[..length];
            }

            var identation = new string('\t', nestingLevel + 1);

            if (PropertyToPrinting.TryGetValue(propertyInfo, out var propertyPrinting))
            {
                return identation + propertyPrinting.DynamicInvoke(propertyValue) + Environment.NewLine;
            }

            if (TypeToPrinting.TryGetValue(propertyInfo.PropertyType, out var typePrinting))
            {
                return identation + propertyInfo.Name + " = " + typePrinting.DynamicInvoke(propertyValue);
            }

            return identation + propertyInfo.Name + " = " + PrintToString(propertyValue, nestingLevel + 1) +
                   Environment.NewLine;
        }

        private string GetFinalTypeRepresentation(object obj)
        {
            return TypeToCultureInfo.TryGetValue(obj.GetType(), out var culture)
                ? ((IFormattable)obj).ToString(null, culture)
                : obj.ToString();
        }

        private string GetEnumerableRepresentation(IEnumerable enumerable, int nestingLevel)
        {
            var arr = enumerable as object[] ?? enumerable.Cast<object>().ToArray();
            var stringValues = arr.Select(e => PrintToString(e, nestingLevel + 1));
            return "[" + string.Join(" ", stringValues) + "]";
        }
    }
}