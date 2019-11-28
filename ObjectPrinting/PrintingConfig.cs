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
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typePrintingConfig = new Dictionary<Type, Delegate>();
        private readonly Dictionary<PropertyInfo, Delegate> propertyToCut = new Dictionary<PropertyInfo, Delegate>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private Dictionary<PropertyInfo, Delegate> propertyPrintingConfig = new Dictionary<PropertyInfo, Delegate>();
        private readonly TOwner value;
        private readonly HashSet<object> printedObjects = new HashSet<object>();
        private readonly int maxNestingLevel = 10;
        public Dictionary<Type, CultureInfo> Cultures => cultures;
        public Dictionary<Type, Delegate> TypePrintingConfig => typePrintingConfig;
        public Dictionary<PropertyInfo, Delegate> PropertyToCut => propertyToCut;

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig(TOwner value)
        {
            this.value = value;
        }

        public PrintingConfig()
        {
            value = default;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            if (!(expression.Body is MemberExpression))
                throw new ArgumentException("Expression must be memberExpression");
            var property = (expression.Body as MemberExpression).Member;
            excludedProperties.Add(property as PropertyInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this, typeof(T));
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> expression)
        {
            var property = (expression.Body as MemberExpression)?.Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, property);
        }

        public string PrintToString(int nestingLevel)
        {
            return PrintToString(value, nestingLevel);
        }

        public string PrintToString(TOwner obj)
        {
            return obj is IEnumerable ? PrintCollection(obj) : PrintToString(obj, 0);
        }

        private string PrintCollection(TOwner obj)
        {
            return new StringBuilder()
                .AppendLine(obj.GetType().GetFullTypeName())
                .Append(PrintCollectionElements(obj as IEnumerable))
                .ToString();
        }

        private string PrintCollectionElements(IEnumerable collection)
        {
            var elements = new StringBuilder();
            foreach (var element in collection)
            {
                elements
                    .Append('\t')
                    .Append(PrintToString(element, 1));
            }

            return elements.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            printedObjects.Add(obj);
            if (obj == null)
                return "null" + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.GetFullTypeName());
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;
                if (nestingLevel > maxNestingLevel)
                    return sb.ToString();
                if (!printedObjects.Contains(obj) || nestingLevel <= 1)
                {
                    sb.Append(indentation).Append(propertyInfo.Name).Append(" = ")
                        .Append(PrintToString(GetValue(propertyInfo, obj), nestingLevel + 1));
                }
            }

            return sb.ToString();
        }

        private object GetValue(PropertyInfo propertyInfo, object obj)
        {
            var value = propertyInfo.GetValue(obj);
            if (TypePrintingConfig.ContainsKey(propertyInfo.PropertyType))
                value = TypePrintingConfig[propertyInfo.PropertyType].DynamicInvoke(value);
            if (propertyPrintingConfig.ContainsKey(propertyInfo))
                value = propertyPrintingConfig[propertyInfo].DynamicInvoke(value);
            if (PropertyToCut.ContainsKey(propertyInfo))
                value = PropertyToCut[propertyInfo].DynamicInvoke(value);
            return value;
        }
    }
}