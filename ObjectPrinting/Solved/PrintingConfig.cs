using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace ObjectPrinting.Solved
{public class PrintingConfig<TOwner>
    {
        private static readonly Type[] finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<string> excludingFields = new HashSet<string>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<string, int> fieldsTrim = new Dictionary<string, int>();
        private readonly HashSet<object> objects = new HashSet<object>();
        private readonly Dictionary<Type, Delegate> alternativeSerialization = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> alternativeSerializationField = new Dictionary<string, Delegate>();

        internal void AddSerialization<TPropType>(Func<TPropType, string> func) => 
            alternativeSerialization[typeof(TPropType)] = func;

        internal void AddSerialization<TPropType>(string fullName, Func<TPropType, string> func) => 
            alternativeSerializationField[fullName] = func;

        internal void AddFieldsTrim(string fullName, int length) => fieldsTrim[fullName] = length;

        internal void AddCulture(Type type, CultureInfo culture) => cultures[type] = culture;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludingFields.Add(memberSelector.GetFullNameProperty());
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, string fullName = "", bool fromCollection = false)
        {
            var type = obj?.GetType();
            objects.Add(obj);

            var result = TryReturnFinalRecursion(obj, type, fullName, nestingLevel, fromCollection);
            if (result != null)
                return result + (!fromCollection ? Environment.NewLine : "");

            var identation = new string('\t', nestingLevel + 1);

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!excludingTypes.Contains(propertyInfo.PropertyType) &&
                    (fullName == null || !excludingFields.Contains(GetFullName())))
                {
                    if (finalTypes.Contains(propertyInfo.DeclaringType) ||
                       !objects.Contains<object>(propertyInfo.GetValue(obj)))
                    sb.Append(identation + propertyInfo.Name + " = " +
                        PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1, GetFullName()));
                }

                string GetFullName() => fullName == null ? null : fullName + '.' + propertyInfo.Name;
            }
            return sb.ToString();
        }

        private string TryReturnFinalRecursion(object obj, Type type, string fullName, int nestingLevel, bool fromCollection)
        {
            if (obj == null)
                return "null";

            if ((fullName != null && alternativeSerializationField.TryGetValue(fullName, out var func)) ||
                alternativeSerialization.TryGetValue(type, out func))
                return func.DynamicInvoke(obj).ToString();

            if (type.IsPrimitive)
                return GetStringFinalType();

            if (obj is ICollection)
                return PrintToStringCollection(obj, nestingLevel);

            return null;

            string GetStringFinalType()
            {
                if (cultures.ContainsKey(type))
                    return GetCulturesResult();
                return obj.ToString();
            }

            string GetCulturesResult()
            {
                if (type == typeof(double))
                    return ((double)obj).ToString(cultures[type]);
                if (type == typeof(int))
                    return ((double)obj).ToString(cultures[type]);
                if (type == typeof(float))
                    return ((float)obj).ToString(cultures[type]);
                if (type == typeof(DateTime))
                    return ((DateTime)obj).ToString(cultures[type]);
                return obj.ToString();
            }
        }

        private string PrintToStringCollection(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            if (obj is IDictionary)
                return GetDictionaryResult();
            return GetCollectionResult();

            string GetDictionaryResult()
            {
                var dict = (IDictionary)obj;
                var keys = dict.Keys;
                var result = new StringBuilder(Environment.NewLine);
                foreach (var k in keys)
                {
                    result.Append(identation + "[Key] = [");
                    result.Append(PrintToString(k, nestingLevel + 1, null, true) + "]");
                    result.Append(", [Value] = [");
                    result.Append(PrintToString(dict[k], nestingLevel + 1, null, true) + "]");
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }

            string GetCollectionResult()
            {
                var array = (ICollection)obj;
                var result = new StringBuilder(Environment.NewLine);
                foreach (var i in array)
                {
                    result.Append(identation + PrintToString(i, nestingLevel + 1, null, true));
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
        }
    }
}