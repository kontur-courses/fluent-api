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
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly Dictionary<Type, Delegate> typeToPrintingMethod;
        private readonly Dictionary<PropertyInfo, Delegate> propertyToPrintingMethod;
        private readonly Dictionary<Type, CultureInfo> cultureForNumber;
        private readonly HashSet<object> processedObjects;

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(short), typeof(sbyte), typeof(long), typeof(byte),
            typeof(ushort), typeof(uint), typeof(ulong), typeof(decimal), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Type[] numbersTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(short), typeof(sbyte), typeof(long), typeof(byte),
            typeof(ushort), typeof(uint), typeof(ulong), typeof(decimal)
        };

        Dictionary<Type, Delegate> IPrintingConfig.TypePrintingMethod => typeToPrintingMethod;
        Dictionary<PropertyInfo, Delegate> IPrintingConfig.PropertyPrintingMethod => propertyToPrintingMethod;
        Dictionary<Type, CultureInfo> IPrintingConfig.CultureForNumber => cultureForNumber;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            typeToPrintingMethod = new Dictionary<Type, Delegate>();
            propertyToPrintingMethod = new Dictionary<PropertyInfo, Delegate>();
            cultureForNumber = new Dictionary<Type, CultureInfo>();
            processedObjects = new HashSet<object>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                ((MemberExpression) memberSelector.Body).Member as PropertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(((MemberExpression) memberSelector.Body).Member as PropertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
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
                return PrintLine("null");

            if (processedObjects.Contains(obj))
                return PrintLine("!cycle!");
            processedObjects.Add(obj);

            var type = obj.GetType();

            if (typeToPrintingMethod.TryGetValue(type, out var @delegate))
                return PrintLine(@delegate.DynamicInvoke(obj));

            if (numbersTypes.Contains(type))
            {
                cultureForNumber.TryGetValue(type, out var culture);
                return PrintLine(obj, culture);
            }

            if (finalTypes.Contains(type))
                return PrintLine(obj);

            if (obj is IEnumerable enumerable)
                return PrintIEnumerable(enumerable, nestingLevel);

            return PrintProperties(obj, nestingLevel);
        }

        private string PrintLine(object data, CultureInfo cultureInfo) =>
            $"{((IFormattable) data).ToString(null, cultureInfo)}{Environment.NewLine}";

        private string PrintLine(object data) => $"{(data)}{Environment.NewLine}";

        private string PrintIEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var result = new StringBuilder();
            foreach (var e in enumerable)
                result.Append($"{e}, ");
            result.Remove(result.Length - 2, 2);
            return result.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var indent = GetIndent(nestingLevel);
            var propertiesResult = new StringBuilder();
            var type = obj.GetType();
            propertiesResult.AppendLine(type.Name);
            var desiredProperties = type.GetProperties()
                .Where(p => !excludedProperties.Contains(p));
            foreach (var propertyInfo in desiredProperties)
            {
                var propertyResult = PrintProperty(propertyInfo, obj, nestingLevel);
                propertiesResult.Append($"{indent}{propertyInfo.Name} = {propertyResult}");
            }

            return propertiesResult.ToString();
        }

        private string PrintProperty(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var propertyValue = propertyInfo.GetValue(obj);
            return propertyToPrintingMethod.TryGetValue(propertyInfo, out var @delegate)
                ? PrintLine(@delegate.DynamicInvoke(propertyValue))
                : PrintToString(propertyValue, nestingLevel + 1);
        }

        private static string GetIndent(int nestingLevel) => new string('\t', nestingLevel + 1);
    }
}