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
    public class PrintingConfig<TOwner>
    {
        private const char Indent = '\t';

        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly List<PropertyInfo> excludedProperties = new List<PropertyInfo>();
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly Dictionary<Type, CultureInfo> typeCultureInfos = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<PropertyInfo, Delegate> propertySerializators = new Dictionary<PropertyInfo, Delegate>();
        private readonly Dictionary<Type, Delegate> typeSerializators = new Dictionary<Type, Delegate>();
//        private readonly Dictionary<PropertyInfo, int> propertyTrimmedLengths = new Dictionary<PropertyInfo, int>();

        private readonly Func<string, int, string> trimString = (str, length) =>
            string.IsNullOrEmpty(str) ? str : str.Substring(0, Math.Min(str.Length, length));

        internal void ChangeSerializationForProperty(PropertyInfo propertyInfo, Delegate serializator)
        {
            propertySerializators[propertyInfo] = serializator;
        }

        internal void ChangeSerializationForType(Type type, Delegate serializator)
        {
            typeSerializators[type] = serializator;
        }

        internal void ChangeCultureInfoForType(Type type, CultureInfo cultureInfo)
        {
            typeCultureInfos[type] = cultureInfo;
        }

        internal void ChangeTrimmedLengthForProperty(PropertyInfo propertyInfo, int length)
        {
//            propertyTrimmedLengths[propertyInfo] = length;
            string TrimFunc(string str) => string.IsNullOrEmpty(str) ? str : str.Substring(0, Math.Min(str.Length, length));
            propertySerializators[propertyInfo] = (Func<string, string>) TrimFunc;
        }

//        private string TrimString(string str, int length)
//        {
//            return string.IsNullOrEmpty(str)
//                ? str
//                : str.Substring(0, Math.Min(str.Length, length));
//        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((PropertyInfo) ((MemberExpression) memberSelector.Body).Member);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var excludedPropertyInfo = ((PropertyInfo)((MemberExpression)memberSelector.Body).Member);
            excludedProperties.Add(excludedPropertyInfo);
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
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string(Indent, nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if (excludedProperties.Contains(propertyInfo)) continue;

                var propertyValue = propertyInfo.GetValue(obj);
                var propertyValueString = string.Empty;

                if (typeCultureInfos.ContainsKey(propertyInfo.PropertyType))
                {
                    var culture = typeCultureInfos[propertyInfo.PropertyType];
                    propertyValueString = string.Format(culture, "{0}", propertyValue) + Environment.NewLine;
                }

                if (typeSerializators.ContainsKey(propertyInfo.PropertyType))
                {
                    propertyValueString =
                        typeSerializators[propertyInfo.PropertyType].DynamicInvoke(propertyValue) + Environment.NewLine;
                }

                if (propertySerializators.ContainsKey(propertyInfo))
                {
                    propertyValueString =
                        propertySerializators[propertyInfo].DynamicInvoke(propertyValue) + Environment.NewLine;
                }

                if (propertyValueString == string.Empty)
                {
                    propertyValueString = PrintToString(propertyValue, nestingLevel + 1);
                }

                sb.Append(identation + propertyInfo.Name + " = " + propertyValueString);
            }
            return sb.ToString();
        }

        private string PrintToStringIEnumerable(object obj, IEnumerable collection)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.Append(
                type.Name +
                Environment.NewLine +
                '{' +
                Environment.NewLine);

            foreach (var item in collection)
            {
                var itemString = PrintToString(item, 0);
                sb.Append(Environment.NewLine);
                sb.Append(itemString);
            }

            sb.Append("}" + Environment.NewLine);

            return sb.ToString();
        }
    }
}