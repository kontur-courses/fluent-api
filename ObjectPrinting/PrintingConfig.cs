using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfigurationHolder
    {
        private readonly Dictionary<Type, CultureInfo> cultureInfos;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly HashSet<Type> excludedTypes;
        private readonly List<Type> finalTypes;
        private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers;
        private readonly Dictionary<string, int> trimedParams;
        private readonly Dictionary<Type, Func<object, string>> typeSerilizers;
        private int nestingLevel;


        public PrintingConfig()
        {
            finalTypes = new List<Type>(new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            });
            cultureInfos = new Dictionary<Type, CultureInfo>();
            excludedProperties = new HashSet<PropertyInfo>();
            excludedTypes = new HashSet<Type>();
            propertySerializers = new Dictionary<PropertyInfo, Func<object, string>>();
            trimedParams = new Dictionary<string, int>();
            typeSerilizers = new Dictionary<Type, Func<object, string>>();
            nestingLevel = 16;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var matchCollection = new Regex(string.Format("{0}.(\\w*)", memberSelector.Parameters[0].Name));
            var excludedProperty = matchCollection.Match(memberSelector.Body.ToString()).Value
                .Replace(memberSelector.Parameters[0].Name + ".", "");
            excludedProperties.Add(typeof(TOwner).GetProperty(excludedProperty));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        internal PrintingConfig<TOwner> WithNestingLevel(int level)
        {
            nestingLevel = level;
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > this.nestingLevel)
                return "Nesting Level Overflow!";

            if (obj == null)
                return "null";

            var pushValue = string.Empty;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var objectType = obj.GetType();

            if (TryFinalizeSerialization(obj, out pushValue))
            {
                sb.Append(new string('\t', nestingLevel) + objectType.Name + "=" + pushValue + Environment.NewLine);
                return sb.ToString();
            }

            sb.AppendLine(objectType.Name);
            if (obj is IEnumerable)
            {
                foreach (var item in obj as IEnumerable)
                    sb.Append(PrintToString(item, nestingLevel + 1));
                return sb.ToString().RemoveEmptyLines();
            }

            foreach (var propertyInfo in objectType.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;
                pushValue = SerializeProperty(propertyInfo, obj, nestingLevel);
                sb.Append(identation + propertyInfo.PropertyType + " " + propertyInfo.Name + "=");
                sb.Append(TrimmProperty(pushValue, propertyInfo) + Environment.NewLine);
            }

            return sb.ToString().RemoveEmptyLines();
        }

        private string SerializeProperty(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            string pushValue;
            if (TryGetPropertySerialization(propertyInfo, obj, out pushValue))
                return pushValue;
            if (TryGetTypeSerialization(propertyInfo, obj, out pushValue))
                return pushValue;
            if (TryFinalizeSerialization(propertyInfo, obj, out pushValue))
                return pushValue;
            pushValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
            return pushValue;
        }

        private bool TryGetPropertySerialization(PropertyInfo propertyInfo, object obj, out string pushValue)
        {
            pushValue = string.Empty;
            if (!propertySerializers.ContainsKey(propertyInfo))
                return false;
            pushValue = propertySerializers[propertyInfo].DynamicInvoke(propertyInfo.GetValue(obj)) as string;
            return true;
        }

        private bool TryGetTypeSerialization(PropertyInfo propertyInfo, object obj, out string pushValue)
        {
            pushValue = string.Empty;
            if (!typeSerilizers.ContainsKey(propertyInfo.PropertyType))
                return false;
            pushValue = typeSerilizers[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)) as string;
            return true;
        }

        private bool TryFinalizeSerialization(PropertyInfo propertyInfo, object obj, out string pushValue)
        {
            pushValue = string.Empty;

            if (!finalTypes.Contains(propertyInfo.PropertyType)) return false;
            var culture = CultureInfo.InvariantCulture;
            if (cultureInfos.ContainsKey(propertyInfo.PropertyType))
                culture = cultureInfos[propertyInfo.PropertyType];

            var propValue = propertyInfo.GetValue(obj);
            if (propValue == null)
            {
                pushValue = "null";
                return false;
            }

            var formatable = propertyInfo.GetValue(obj) as IFormattable;
            if (formatable == null)
                pushValue = propValue.ToString();
            else
                pushValue = (propValue as IFormattable).ToString(null, culture);
            return true;
        }

        private bool TryFinalizeSerialization(object obj, out string pushValue)
        {
            pushValue = string.Empty;
            if (!finalTypes.Contains(obj.GetType()))
                return false;
            if (cultureInfos.ContainsKey(obj.GetType()) && obj is IFormattable)
                pushValue = (obj as IFormattable).ToString(null, cultureInfos[obj.GetType()]);
            else
                pushValue = obj.ToString();
            return true;
        }

        private string TrimmProperty(string str, PropertyInfo propertyInfo)
        {
            if (trimedParams.ContainsKey(propertyInfo.Name))
                str = string.Join("", str.Take(trimedParams[propertyInfo.Name]));
            return str;
        }

        #region InterfaceRealization

        HashSet<Type> IPrintingConfigurationHolder.ExcludedTypes
        {
            get { return excludedTypes; }
        }

        HashSet<PropertyInfo> IPrintingConfigurationHolder.ExcludedProperties
        {
            get { return excludedProperties; }
        }

        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfigurationHolder.PropertySerializers
        {
            get { return propertySerializers; }
        }

        Dictionary<Type, Func<object, string>> IPrintingConfigurationHolder.TypeSerilizers
        {
            get { return typeSerilizers; }
        }

        Dictionary<Type, CultureInfo> IPrintingConfigurationHolder.CultureInfos
        {
            get { return cultureInfos; }
        }

        Dictionary<string, int> IPrintingConfigurationHolder.TrimedParams
        {
            get { return trimedParams; }
        }

        #endregion
    }
}