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
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly Dictionary<Type, Delegate> typeToFormatter;
        private readonly Dictionary<PropertyInfo, Delegate> propertyToFormatter;
        private readonly Dictionary<PropertyInfo, int> stringPropertyToLength;
        private readonly Dictionary<Type, CultureInfo> numberTypeToCulture;
        private readonly Type[] finalTypes;
        private Func<(int nestingLevel, string propertyName, string formattedProperty), string > 
            printFormattedProperty;

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypeToFormatter => typeToFormatter;
        Dictionary<PropertyInfo, Delegate> 
            IPrintingConfig<TOwner>.PropertyToFormatter => propertyToFormatter;
        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.PropertyToLength => stringPropertyToLength;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.DigitTypeToCulture => numberTypeToCulture;

        private readonly TOwner owner;

        public PrintingConfig(TOwner obj)
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            typeToFormatter = new Dictionary<Type, Delegate>();
            propertyToFormatter = new Dictionary<PropertyInfo, Delegate>();
            stringPropertyToLength = new Dictionary<PropertyInfo, int>();
            numberTypeToCulture = new Dictionary<Type, CultureInfo>();
            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            printFormattedProperty = (args) => 
            $"{ new string('\t', args.nestingLevel + 1) }{args.propertyName} = {args.formattedProperty}";

            owner = obj;
        }

        public string Print()
        {
            return PrintToString(owner, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;
            if (obj is ICollection collection)
                return PrintCollection(collection, nestingLevel);

            return PrintProperties(obj, nestingLevel);
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            lock (collection.SyncRoot)
            {
                foreach (var obj in collection)
                    sb.Append(PrintToString(obj, nestingLevel));
            }
            return sb.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var formattedProperty = FormatProperty(obj, propertyInfo, nestingLevel);
                if (formattedProperty == null)
                    continue;
                sb.Append(printFormattedProperty((nestingLevel, propertyInfo.Name, formattedProperty)));
            }
            return sb.ToString();
        }

        private string FormatProperty(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (excludedProperties.Contains(propertyInfo) ||
                excludedTypes.Contains(propertyInfo.PropertyType))
                return null;

            var value = propertyInfo.GetValue(obj);
            var type = propertyInfo.PropertyType;

            if (numberTypeToCulture.ContainsKey(type))
                return String.Format(numberTypeToCulture[type], "{0}", value) + Environment.NewLine;

            var formattedProperty = value as string;
            if (typeToFormatter.ContainsKey(type))
                formattedProperty = typeToFormatter[type].DynamicInvoke(value) as string;
            else if (propertyToFormatter.ContainsKey(propertyInfo))
                formattedProperty = 
                    propertyToFormatter[propertyInfo].DynamicInvoke(value) as string;
            if (stringPropertyToLength.ContainsKey(propertyInfo) && formattedProperty != null)
                formattedProperty = formattedProperty.Substring(0,
                    Math.Min(formattedProperty.Length, stringPropertyToLength[propertyInfo]));

            if (formattedProperty != null)
                return formattedProperty + Environment.NewLine;

            return PrintToString(value, nestingLevel + 1);

        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression;
            excludedProperties.Add(memberExpr.Member as PropertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> ChangeFormattedPropertyPrinting(
            Func<(int nestingLevel, string propertyName, string formattedProperty), string> newPrint)
        {
            printFormattedProperty = newPrint;
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> ForProperty<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ForProperty<T>(Expression<Func<TOwner, T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression;
            return new PropertyPrintingConfig<TOwner, T>(this, memberExpr.Member as PropertyInfo);
        }
    }
}