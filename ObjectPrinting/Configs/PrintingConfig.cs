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
        private HashSet<Type> excludedTypes;

        private HashSet<PropertyInfo> excludedProperties;

        private Dictionary<Type, Delegate> typeSerializers;

        private Dictionary<Type, CultureInfo> typeCultureInfo;

        private Dictionary<PropertyInfo, Delegate> propertySerializers;

        private Dictionary<PropertyInfo, int> propertyTrimmers;

        private int nestingLimit = 10;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            typeSerializers = new Dictionary<Type, Delegate>();
            typeCultureInfo = new Dictionary<Type, CultureInfo>();
            propertySerializers = new Dictionary<PropertyInfo, Delegate>();
            propertyTrimmers = new Dictionary<PropertyInfo, int>();
        }

        public TypePrintingConfig<TOwner, TType> Printing<TType>()
        {
            return new TypePrintingConfig<TOwner, TType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo =
                ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo =
                ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            excludedProperties.Add(propertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> SetNestingLimit(int limit)
        {
            nestingLimit = limit;
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, PropertyInfo propertyInfo = null)
        {
            if (nestingLevel == nestingLimit)
                return "...Can't go further due to nesting limit...";

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type) || type.GetProperties().Length == 0)
                return ApplySettings(obj, propertyInfo) + Environment.NewLine;
            else if (IsKeyValuePairType(type))
                return PrintKeyValuePair(obj, nestingLevel);
            else if (obj is IEnumerable enumerable)
                return PrintEnumerable(enumerable, nestingLevel);
            return PrintNestedObject(obj, nestingLevel);
        }

        private bool IsKeyValuePairType(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

        private string PrintKeyValuePair(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var bracesIdentation = new string('\t', nestingLevel);
            var kvpProperties = type.GetProperties();
            var key = PrintToString(kvpProperties[0].GetValue(obj), nestingLevel + 1);
            var value = PrintToString(kvpProperties[1].GetValue(obj), nestingLevel + 1);
            return "{" +
                Environment.NewLine +
                bracesIdentation + "Key:" +
                Environment.NewLine +
                identation + key +
                bracesIdentation + "Value:" +
                Environment.NewLine +
                identation + value +
                bracesIdentation + "}" + Environment.NewLine;
        }

        private string PrintEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var enumerableItems = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var bracesIdentation = new string('\t', nestingLevel);
            foreach (var item in enumerable)
                enumerableItems.Append(identation + PrintToString(item, nestingLevel + 1));
            return "[" +
                    Environment.NewLine +
                    enumerableItems +
                    bracesIdentation +
                    "]" +
                    Environment.NewLine;
        }

        private string ApplySettings(object obj, PropertyInfo propertyInfo)
        {
            var result = obj.ToString();
            var type = obj.GetType();
            if (typeCultureInfo.ContainsKey(type))
                result = string.Format(typeCultureInfo[type], "{0}", obj);
            if (typeSerializers.ContainsKey(type))
                result = typeSerializers[type].DynamicInvoke(obj).ToString();
            if (propertyInfo != null)
            {
                if (propertySerializers.ContainsKey(propertyInfo))
                    result = propertySerializers[propertyInfo].DynamicInvoke(obj).ToString();
                if (propertyTrimmers.ContainsKey(propertyInfo))
                {
                    var trimLength = propertyTrimmers[propertyInfo];
                    result = result.Length < trimLength ?
                        result : result.Substring(0, trimLength);
                }
            }
            return result;
        }

        private string PrintNestedObject(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var includedProperties = type.GetProperties()
                .Where(prop => !excludedTypes.Contains(prop.PropertyType))
                .Where(prop => !excludedProperties.Contains(prop));
            foreach (var propInfo in includedProperties)
            {
                sb.Append(identation + propInfo.Name + " = " +
                    PrintToString(propInfo.GetValue(obj),
                              nestingLevel + 1, propInfo));
            }
            return sb.ToString();
        }

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypeSerializers => typeSerializers;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.TypeCultureInfo => typeCultureInfo;
        Dictionary<PropertyInfo, Delegate> IPrintingConfig<TOwner>.PropertySerializers => propertySerializers;
        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.PropertyTrimmers => propertyTrimmers;
    }
}