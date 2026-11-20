using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<string> excludedFields = new();
        private readonly Dictionary<Type, Func<object, string>> typesPrinting = new();
        private readonly Dictionary<string, Func<object, string>> fieldsPrinting = new();
        private readonly Dictionary<Type, CultureInfo> typesCultureInfo = new();
        private readonly Dictionary<string, int> stringTypesTrim = new();
        private static readonly Type[] primitiveTypes = new[] {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(decimal),
            typeof(long), typeof(short), typeof(byte), typeof(Guid)
        };
        private Dictionary<object, int> printedObject = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
        private int recursionLimit = 10;

        public PrintingConfig<TOwner> WithRecursionLimit(int limit)
        {
            if (limit < 0)
                throw new ArgumentException("Лимит рекурсии не может быть отрицательным");
            recursionLimit = limit;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            if (typeof(TPropType) == typeof(TOwner))
            {
                throw new InvalidOperationException("Нельзя настроить сериализацию для класса сериализации");
            }
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberExpression.Member.Name);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            excludedFields.Add(memberExpression.Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            if (typeof(TPropType) == typeof(TOwner))
            {
                throw new InvalidOperationException("Нельзя исключить из сериализации сам класс сериализации");
            }

            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            printedObject = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, string? fieldName=null)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (printedObject.ContainsKey(obj))
            {
                printedObject[obj]++;
                if (printedObject[obj] > recursionLimit)
                {
                    return "<already been>" + Environment.NewLine;
                }
            }
            else
            {
                printedObject[obj] = 1;
            }

            var objType = obj.GetType();

            if (fieldName != null && fieldsPrinting.TryGetValue(fieldName, out var fieldFunc))
            {
                return fieldFunc(obj) + Environment.NewLine;
            }

            if (typesPrinting.TryGetValue(objType, out var typeFunc))
            {
                return typeFunc(obj) + Environment.NewLine;
            }

            if (primitiveTypes.Contains(objType))
            {
                if (typesCultureInfo.TryGetValue(objType, out var typeCulture) && obj is IFormattable formattable)
                    return formattable.ToString(null, typeCulture) + Environment.NewLine;

                if (obj is string strObj && fieldName != null && stringTypesTrim.TryGetValue(fieldName, out int maxLen))
                {
                    return (strObj.Length > maxLen ? strObj.Substring(0, maxLen) : strObj) + Environment.NewLine;
                }
                return obj + Environment.NewLine;
            }

            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                dynamic kvp = obj;
                return PrintKeyValuePairToString(kvp.Key, kvp.Value, nestingLevel);
            }

            if (obj is IEnumerable enumerable)
            {
                return PrintEnumerableToString(enumerable, nestingLevel);
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder(objType.Name + " {" + Environment.NewLine);
            foreach (var propertyInfo in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (excludedFields.Contains(propertyInfo.Name) || excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = "
                    + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, propertyInfo.Name));
            }
            foreach (var fieldInfo in objType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (excludedFields.Contains(fieldInfo.Name) || excludedTypes.Contains(fieldInfo.FieldType))
                    continue;

                sb.Append(identation + fieldInfo.Name + " = "
                    + PrintToString(fieldInfo.GetValue(obj), nestingLevel + 1, fieldInfo.Name));
            }
            sb.Append(new string('\t', nestingLevel) + "}" + Environment.NewLine);
            return sb.ToString();
        }

        internal void AddNewTypePrintingFunc(Type type, Func<object, string> printingFunc)
        {
            typesPrinting[type] = printingFunc;
        }

        internal void AddNewFieldPrintingFunc(string fieldName, Func<object, string> printingFunc)
        {
            fieldsPrinting[fieldName] = printingFunc;
        }

        internal void AddNewTypeCultureInfo(Type type, CultureInfo cultureInfo)
        {
            typesCultureInfo[type] = cultureInfo;
        }

        internal void AddNewFieldTrim(string fieldName, int maxLength)
        {
            stringTypesTrim[fieldName] = maxLength;
        }

        internal string PrintEnumerableToString(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder("{" + Environment.NewLine);
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation + "Count = " + enumerable.Cast<object>().Count() + Environment.NewLine);
            var index = 0;
            foreach (var item in enumerable)
            {
                sb.Append(identation + $"[{index}] = ");
                sb.Append(PrintToString(item, nestingLevel + 1));
                index++;

            }
            sb.Append(new string('\t', nestingLevel) + "}" + Environment.NewLine);
            return sb.ToString();
        }

        internal string PrintKeyValuePairToString(object key, object value, int nestingLevel)
        {
            var sb = new StringBuilder("{" + Environment.NewLine);
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation + "Key = " + PrintToString(key, nestingLevel + 1));
            sb.Append(identation + "Value = " + PrintToString(value, nestingLevel + 1));
            sb.Append(new string('\t', nestingLevel) + "}" + Environment.NewLine);
            return sb.ToString();
        }
    }
}