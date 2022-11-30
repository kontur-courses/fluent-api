using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> specialFinalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(decimal)
        };

        private int nestingCollection;
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludingProperties = new HashSet<MemberInfo>();
        internal readonly Dictionary<Type, Delegate> TypesForPrintWithSpec = new Dictionary<Type, Delegate>();

        internal readonly Dictionary<MemberInfo, Delegate> PropertiesForPrintWithSpec =
            new Dictionary<MemberInfo, Delegate>();

        private readonly HashSet<object> printedObjects = new HashSet<object>();
        internal readonly Dictionary<MemberInfo, int> PropertyLenForString = new Dictionary<MemberInfo, int>();

        private static MemberInfo GetPropertyInfoFromExpression<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            ((MemberExpression)memberSelector.Body).Member as MemberInfo;

        private string GetIDictionaryString(IDictionary iDictionary, int nestingLevel, int maxNesting)
        {
            nestingCollection++;
            var values = (from object key in iDictionary.Keys
                let stringKey = GetValueString(key, nestingLevel + 1, maxNesting, "")
                let stringValue = GetValueString(iDictionary[key], nestingLevel + 1, maxNesting, "")
                where !string.IsNullOrEmpty(stringValue)
                select $"[{stringKey}] = {stringValue}").ToList();
            var finalString = "[" + string.Join(", ", values) + "]";
            nestingCollection--;
            return nestingCollection == 0 ? finalString + Environment.NewLine : finalString;
        }

        private string GetIEnumerableString(IEnumerable enumerable, int nestingLevel, int maxNesting)
        {
            nestingCollection++;
            var values = (from object enumerableObject in enumerable
                select GetValueString(enumerableObject, nestingLevel + 1, maxNesting, "")).ToList();
            var finalString = "[" + string.Join(", ", values) + "]";
            nestingCollection--;
            return nestingCollection == 0 ? finalString + Environment.NewLine : finalString;
        }

        private string GetValueString(object obj, int nestingLevel, int maxNesting, string newLine = null,
            MemberInfo property = null)
        {
            newLine ??= Environment.NewLine;
            if (TryGetDefaultString(obj, property, out var valueString, newLine))
                return valueString;
            if (TryGetCollectionString(obj, nestingLevel, maxNesting, out var iEnumerableString))
                return iEnumerableString;
            var stringBuilder = new StringBuilder();
            var exType = false;
            var type = obj.GetType();
            if (!excludingTypes.Contains(type))
                stringBuilder.Append(type.Name + newLine);
            else
                exType = true;
            if (printedObjects.Contains(obj))
                return "Cyclic reference" + newLine;
            var tString = newLine == Environment.NewLine ? new string('\t', nestingLevel + 1) : "; ";
            var fieldsAndProperties = type.GetProperties()
                .Select(x => (MemberInfo)x)
                .Concat(type.GetFields());
            foreach (var fieldInfo in fieldsAndProperties)
                if (exType || AppendFieldOrProperty(obj, nestingLevel, maxNesting, newLine, fieldInfo, stringBuilder,
                        tString))
                    break;
            return stringBuilder.ToString();
        }

        private static bool IsSimple(Type type)
        {
            return type.IsPrimitive
                   || type.IsEnum
                   || specialFinalTypes.Contains(type);
        }

        private bool AppendFieldOrProperty(object obj, int nestingLevel, int maxNesting, string newLine,
            MemberInfo memberInfo,
            StringBuilder stringBuilder, string tString)
        {
            if (!obj.GetType().IsPrimitive)
                printedObjects.Add(obj);
            if (nestingLevel == maxNesting)
                return true;
            var value = "";
            switch (memberInfo)
            {
                case FieldInfo fieldInfo
                    when !TryAppendField(obj, nestingLevel, maxNesting, newLine, fieldInfo, out value):
                case PropertyInfo propertyInfo
                    when !TryAppendProperty(obj, nestingLevel, maxNesting, newLine, propertyInfo, out value):
                    return false;
            }

            if (string.IsNullOrEmpty(value)) return false;
            AppendLineToStringBuilder(stringBuilder, tString, memberInfo, value);
            return false;
        }

        private bool TryAppendField(object obj, int nestingLevel, int maxNesting, string newLine, FieldInfo fieldInfo,
            out string value)
        {
            value = null;
            if (excludingTypes.Contains(fieldInfo.FieldType) || excludingProperties.Contains(fieldInfo))
                return false;
            value = GetValueString(fieldInfo.GetValue(obj), nestingLevel + 1, maxNesting, newLine, fieldInfo);
            return true;
        }

        private bool TryAppendProperty(object obj, int nestingLevel, int maxNesting, string newLine,
            PropertyInfo propertyInfo,
            out string value)
        {
            value = null;
            if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingProperties.Contains(propertyInfo))
                return false;
            value = GetValueString(propertyInfo.GetValue(obj), nestingLevel + 1, maxNesting, newLine, propertyInfo);
            return true;
        }

        private static void AppendLineToStringBuilder(StringBuilder stringBuilder, string tString,
            MemberInfo propertyInfo,
            string value)
        {
            stringBuilder.Append(tString);
            stringBuilder.Append(propertyInfo.Name);
            stringBuilder.Append(" = ");
            stringBuilder.Append(value);
        }

        private bool TryGetCollectionString(object obj, int nestingLevel, int maxNesting, out string iEnumerableString)
        {
            switch (obj)
            {
                case IDictionary dictionary:
                    iEnumerableString = GetIDictionaryString(dictionary, nestingLevel + 1, maxNesting);
                    return true;
                case IEnumerable list:
                    iEnumerableString = GetIEnumerableString(list, nestingLevel + 1, maxNesting);
                    return true;
            }

            iEnumerableString = null;
            return false;
        }

        private bool TryGetDefaultString(object obj, MemberInfo property, out string valueString, string newLine)
        {
            valueString = null;
            if (obj is null)
                return true;
            var objType = obj.GetType();
            if (!IsSimple(objType))
                return false;
            var isTrimmed = TryTrimString(obj, property, ref valueString);
            valueString = GetString(obj, valueString, objType, property, isTrimmed);
            valueString += newLine;
            return true;
        }

        private string GetString(object obj, string valueString, Type objType, MemberInfo property, bool isTrimmed)
        {
            if (property != null && PropertiesForPrintWithSpec.ContainsKey(property))
                valueString = DynamicInvokePropertiesPrint(isTrimmed ? valueString : obj, property);
            else if (TypesForPrintWithSpec.ContainsKey(objType))
                valueString = DynamicInvokeTypesPrint(isTrimmed ? valueString : obj, objType);
            else if (!isTrimmed)
                valueString = obj.ToString();
            return valueString;
        }

        private string DynamicInvokePropertiesPrint(object valueString, MemberInfo property)
        {
            return (string)PropertiesForPrintWithSpec[property].DynamicInvoke(valueString);
        }

        private string DynamicInvokeTypesPrint(object valueString, Type type)
        {
            return (string)TypesForPrintWithSpec[type].DynamicInvoke(valueString);
        }

        private bool TryTrimString(object obj, MemberInfo property, ref string valueString)
        {
            if (property == null || !PropertyLenForString.ContainsKey(property) || !(obj is string s) ||
                s.Length <= PropertyLenForString[property]) return false;
            valueString = s[..PropertyLenForString[property]];
            return true;
        }

        public string PrintToString(TOwner obj, int maxNesting = -1, int nestingLevel = 0)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            return GetValueString(obj, nestingLevel, maxNesting);
        }

        /// <summary>
        /// Include fields by type TPropType for next serialization
        /// </summary>
        /// <typeparam name="TPropType"> Generic parameter for serialization TPropType </typeparam>
        /// <returns>PropertyConfig&lt;TOwner, TPropType&gt;</returns>
        public PropertyConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyConfig<TOwner, TPropType>(this);
        }

        /// <summary>
        /// Include field by property TPropType for next serialization
        /// </summary>
        /// <typeparam name="TPropType"> Generic parameter for serialization TPropType </typeparam>
        /// <returns>PropertyConfig&lt;TOwner, TPropType&gt;</returns>
        public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyConfig<TOwner, TPropType>(this, GetPropertyInfoFromExpression(memberSelector));
        }

        /// <summary>
        /// Exclude property from <typeparam name="TPropType">Expression&lt;Func&lt;TOwner, TPropType&gt;&gt;</typeparam> 
        /// </summary>
        /// <typeparam name="TPropType"> Generic parameter for serialization TPropType </typeparam>
        /// <returns> PrintingConfig&lt;TOwner&gt; </returns>
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludingProperties.Add(GetPropertyInfoFromExpression(memberSelector));
            return this;
        }

        /// <summary>
        /// Exclude all fields of <typeparam name="TPropType">TPropType</typeparam> from PrintingConfig 
        /// </summary>
        /// <typeparam name="TPropType"> Generic parameter for serialization TPropType </typeparam>
        /// <returns> PrintingConfig&lt;TOwner&gt; </returns>

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }
    }
}