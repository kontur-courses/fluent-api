using System;
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
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly HashSet<object> printedObjects = new HashSet<object>();
        private int depth = 10;
        private readonly Dictionary<Type, Delegate> typeSerializationFormats = new Dictionary<Type, Delegate>();
        private readonly Dictionary<PropertyInfo, Delegate> propertySerializationFormats = new Dictionary<PropertyInfo, Delegate>();
        private readonly Dictionary<PropertyInfo, Delegate> propertyPostProduction = new Dictionary<PropertyInfo, Delegate>();
        private readonly Dictionary<Type, CultureInfo> cultureInfo = new Dictionary<Type, CultureInfo>();
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig<TOwner> SetDepth(int depth)
        {
            this.depth = depth;
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
            excludedProperties.Add(typeof(TOwner).GetProperty(propertyName));
            return this;
        }
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            printedObjects.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (TryPrintFinalType(obj, out var result))
                return result;

            if (printedObjects.Contains(obj))
                return "Already printed" + Environment.NewLine;

            if (nestingLevel > depth)
                return "";
            printedObjects.Add(obj);

            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo) ||
                    excludedTypes.Contains(propertyInfo.PropertyType))
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
                result = IntToString((int) obj) + Environment.NewLine;
            else if (type == typeof(double))
                result = DoubleToString((double) obj) + Environment.NewLine;
            else if (type == typeof(float))
                result = FloatToString((float) obj) + Environment.NewLine;
            else
                result = obj +Environment.NewLine;
            return true;
        }

        private string IntToString(int value)
        {
            return cultureInfo.ContainsKey(typeof(int)) ? value.ToString(cultureInfo[typeof(int)]) : value.ToString();
        }

        private string FloatToString(float value)
        {
            return cultureInfo.ContainsKey(typeof(float)) ? value.ToString(cultureInfo[typeof(float)]) : value.ToString();
        }

        private string DoubleToString(double value)
        {
            return cultureInfo.ContainsKey(typeof(double)) ? value.ToString(cultureInfo[typeof(double)]) : value.ToString();
        }

        void IPrintingConfig<TOwner>.AddPropertySerializationFormat(PropertyInfo property, Delegate format)
        {
            propertySerializationFormats[property] = format;
        }

        void IPrintingConfig<TOwner>.AddTypeSerializationFormat(Type type, Delegate format)
        {
            typeSerializationFormats[type] = format;
        }

        void IPrintingConfig<TOwner>.AddPostProduction(PropertyInfo property, Delegate format)
        {
            propertyPostProduction[property] = format;
        }

        void IPrintingConfig<TOwner>.AddCultureInfo(Type type, CultureInfo cultureInfo)
        {
            this.cultureInfo[type] = cultureInfo;
        }
    }
}