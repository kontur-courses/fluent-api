using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool), typeof(decimal), typeof(long),
            typeof(byte), typeof(sbyte), typeof(short), typeof(char), typeof(ushort), typeof(uint), typeof(ulong),
            typeof(ushort)
        };

        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludingProperties = new HashSet<PropertyInfo>();
        public readonly Dictionary<Type, Delegate> TypesForPrintWithSpec = new Dictionary<Type, Delegate>();

        public readonly Dictionary<PropertyInfo, Delegate> PropertiesForPrintWithSpec =
            new Dictionary<PropertyInfo, Delegate>();

        private readonly HashSet<object> printedObjects = new HashSet<object>();
        public readonly Dictionary<PropertyInfo, int> PropertyLenForString = new Dictionary<PropertyInfo, int>();

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

        private static PropertyInfo GetPropertyInfoFromExpression<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return ((MemberExpression)memberSelector.Body).Member as PropertyInfo;

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

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }


        public string PrintToString(TOwner obj, int maxNesting = -1)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            return GetValueString(obj, 0, maxNesting);
        }

        private string GetValueString(object obj, int nestingLevel, int maxNesting, PropertyInfo property = null)
        {
            if (ReturnDefaultString(obj, property, out var valueString))
                return valueString;
            var stringBuilder = new StringBuilder();
            var type = obj.GetType();
            stringBuilder.AppendLine(type.Name);
            if (printedObjects.Contains(obj))
                return "Cyclic reference" + Environment.NewLine;
            foreach (var propertyInfo in type.GetProperties())
            {
                printedObjects.Add(obj);
                if (nestingLevel == maxNesting)
                    break;
                if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingProperties.Contains(propertyInfo))
                    continue;

                var value = GetValueString(propertyInfo.GetValue(obj), nestingLevel + 1, maxNesting, propertyInfo);
                if (string.IsNullOrEmpty(value)) continue;
                stringBuilder.Append(new string('\t', nestingLevel + 1));
                stringBuilder.Append(propertyInfo.Name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }

        private bool ReturnDefaultString(object obj, PropertyInfo property, out string valueString)
        {
            valueString = null;
            if (obj is null)
                return true;
            var objType = obj.GetType();
            if (!FinalTypes.Contains(objType))
                return false;
            var isTrimmed = TrimString(obj, property, ref valueString);
            valueString = GetString(obj, valueString, objType, property, isTrimmed);
            valueString += Environment.NewLine;
            return true;
        }

        private string GetString(object obj, string valueString, Type objType, PropertyInfo property, bool isTrimmed)
        {
            if (PropertiesForPrintWithSpec.ContainsKey(property))
            {
                if (isTrimmed)
                    valueString = (string)PropertiesForPrintWithSpec[property].DynamicInvoke(valueString);
                else
                    valueString = (string)PropertiesForPrintWithSpec[property].DynamicInvoke(obj);
            }
            else if (TypesForPrintWithSpec.ContainsKey(objType))
            {
                if (isTrimmed)
                    valueString = (string)TypesForPrintWithSpec[objType].DynamicInvoke(valueString);
                else
                    valueString = (string)TypesForPrintWithSpec[objType].DynamicInvoke(obj);
            }
            else if (!isTrimmed)
                valueString = obj.ToString();

            return valueString;
        }

        private bool TrimString(object obj, PropertyInfo property, ref string valueString)
        {
            if (property == null || !PropertyLenForString.ContainsKey(property) || !(obj is string s) ||
                s.Length <= PropertyLenForString[property]) return false;
            valueString = s[..PropertyLenForString[property]];
            return true;
        }
    }
}