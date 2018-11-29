using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private ImmutableHashSet<Type> excludedTypes = ImmutableHashSet<Type>.Empty;
        private ImmutableHashSet<PropertyInfo> excludedProperties = ImmutableHashSet<PropertyInfo>.Empty;
        private ImmutableHashSet<object> printedObjects = ImmutableHashSet<object>.Empty;
        private int depth = 10;
        private int maxElements = 10;
        private ImmutableDictionary<Type, Delegate> typeSerializationFormats = ImmutableDictionary<Type, Delegate>.Empty;
        private ImmutableDictionary<PropertyInfo, Delegate> propertySerializationFormats = ImmutableDictionary<PropertyInfo, Delegate>.Empty;
        private ImmutableDictionary<PropertyInfo, Delegate> propertyPostProduction = ImmutableDictionary<PropertyInfo, Delegate>.Empty;
        private ImmutableDictionary<Type, CultureInfo> cultureInfo = ImmutableDictionary<Type, CultureInfo>.Empty;
        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig<TOwner> WithDepth(int newDepth)
        {
            depth = newDepth;
            return this;
        }

        public PrintingConfig<TOwner> MaxElements(int newMaxElements)
        {
            maxElements = newMaxElements;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, property);
        }
        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            if (!(property.Body is MemberExpression body))
                throw new ArgumentException();
            var propertyName = body.Member.Name;
            excludedProperties = excludedProperties.Add(typeof(TOwner).GetProperty(propertyName));
            return this;
        }
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes = excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            printedObjects = printedObjects.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (TryPrintFinalType(obj, out var result))
                return result;

            if (printedObjects.Contains(obj))
                return "Already printed " + obj.GetType() + Environment.NewLine;

            return nestingLevel > depth ? string.Empty : PrintObject(obj, nestingLevel);
        }

        private string PrintObject(object obj, int nestingLevel)
        {
            if (!finalTypes.Contains(obj.GetType()))
                printedObjects = printedObjects.Add(obj);

            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            if (TryPrintElements(obj, nestingLevel, out var result))
                sb.Append(result);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (IsExcluded(propertyInfo))
                    continue;

                sb.Append(identation);
                if (TryPrint(obj, propertyInfo, out result))
                    sb.Append(result);
                else
                    sb.Append(propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }

            return sb.ToString();
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return excludedProperties.Contains(propertyInfo) ||
                   excludedTypes.Contains(propertyInfo.PropertyType);
        }

        private bool TryPrintElements(object obj, int nestingLevel, out string result)
        {
            result = null;
            if (!(obj is IEnumerable enumerable))
                return false;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.Append(identation + "Elements:" + Environment.NewLine);
            sb.Append(identation +  "{" + Environment.NewLine);
            var count = 0;
            foreach (var e in enumerable)
            {
                sb.Append(identation + identation + PrintToString(e, nestingLevel + 1));
                count++;
                if (count <= maxElements || maxElements == 0)
                    continue;
                sb.Append(identation + identation + $"First {count} elements...{Environment.NewLine}");
                break;
            }

            sb.Append(identation + "}" + Environment.NewLine);

            result = sb.ToString();
            return true;

        }

        private bool TryPrint(object obj, PropertyInfo propertyInfo, out string result)
        {
            result = null;

            if (propertySerializationFormats.ContainsKey(propertyInfo))
                result = propertySerializationFormats[propertyInfo]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();
            else if (typeSerializationFormats.ContainsKey(propertyInfo.PropertyType))
                result = typeSerializationFormats[propertyInfo.PropertyType]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();

            if (result == null)
                return false;

            if (propertyPostProduction.ContainsKey(propertyInfo))
                result = propertyPostProduction[propertyInfo]
                    .DynamicInvoke(result).ToString();

            result += Environment.NewLine;
            return true;

        }

        private bool TryPrintFinalType(object obj, out string result)
        {
            var type = obj.GetType();
            result = null;
            if (!finalTypes.Contains(type))
                return false;

            if (type == typeof(int))
                result = ((int) obj).ToString(GetCulture(type));
            else if (type == typeof(double))
                result = ((double)obj).ToString(GetCulture(type));
            else if (type == typeof(float))
                result = ((float)obj).ToString(GetCulture(type));
            else
                result = obj.ToString();
            result += Environment.NewLine;
            return true;
        }

        private CultureInfo GetCulture(Type type)
        {
            return cultureInfo.ContainsKey(type) ? cultureInfo[type] : CultureInfo.InvariantCulture;
        }

        void IPrintingConfig.AddPropertySerializationFormat(PropertyInfo property, Delegate format)
        {
            propertySerializationFormats = propertySerializationFormats.SetItem(property, format);
        }

        void IPrintingConfig.AddTypeSerializationFormat(Type type, Delegate format)
        {
            typeSerializationFormats = typeSerializationFormats.SetItem(type, format);
        }

        void IPrintingConfig.AddPostProduction(PropertyInfo property, Delegate format)
        {
            propertyPostProduction = propertyPostProduction.SetItem(property, format);
        }

        void IPrintingConfig.AddCultureInfo(Type type, CultureInfo info)
        {
            cultureInfo = cultureInfo.SetItem(type, info);
        }
    }
}