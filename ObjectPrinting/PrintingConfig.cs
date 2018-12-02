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

        private readonly List<PropertyInfo> excludedProperties = new List<PropertyInfo>();
        private readonly List<Type> excludedTypes = new List<Type>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Dictionary<PropertyInfo, Delegate> propertySerializators =
            new Dictionary<PropertyInfo, Delegate>();

        private readonly Dictionary<PropertyInfo, int> propertyTrimmedLengths = new Dictionary<PropertyInfo, int>();
        private readonly Dictionary<Type, CultureInfo> typeCultureInfos = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typeSerializators = new Dictionary<Type, Delegate>();
        private int collectionLookDepth;
        private int propertiesLookDepth;

        public PrintingConfig()
        {
            propertiesLookDepth = 50;
            CollectionLookDepth = 50;
        }

        private bool CanThrowWhenTooDeep { get; set; }

        public int PropertiesLookDepth
        {
            get => propertiesLookDepth;
            private set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Depth should be positive");
                propertiesLookDepth = value;
            }
        }

        public int CollectionLookDepth
        {
            get => collectionLookDepth;
            private set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Depth should be positive");
                collectionLookDepth = value;
            }
        }

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

        internal void ChangeTrimmedLengthForProperty(PropertyInfo propertyInfo, int maxLen)
        {
            propertyTrimmedLengths[propertyInfo] = maxLen;
        }

        private static string TrimString(string str, int maxLen)
        {
            return string.IsNullOrEmpty(str)
                ? str
                : str.Substring(0, Math.Min(str.Length, maxLen));
        }

        public PrintingConfig<TOwner> SetCollectionLookDepthTo(int depth)
        {
            CollectionLookDepth = depth;
            return this;
        }

        public PrintingConfig<TOwner> SetPropetiesLookDepthTo(int depth, bool thowsException = false)
        {
            PropertiesLookDepth = depth;
            CanThrowWhenTooDeep = thowsException;
            return this;
        }

        public PrintingConfig<TOwner> CanThrowExceptionWhenTooDeep(bool thowsException = true)
        {
            CanThrowWhenTooDeep = thowsException;
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo) ((MemberExpression) memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var excludedPropertyInfo = (PropertyInfo) ((MemberExpression) memberSelector.Body).Member;
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
            if (obj == null) return "null";

            var type = obj.GetType();
            if (finalTypes.Contains(type)) return obj.ToString();

            if (nestingLevel > PropertiesLookDepth)
                return CanThrowWhenTooDeep
                    ? throw new InvalidOperationException("Printing too deep (possible recursion)")
                    : "...";

            var indentation = new string(Indent, nestingLevel + 1);
            var sb = new StringBuilder();

            if (obj is IEnumerable collection)
            {
                sb.Append(PrintToStringIEnumerable(obj, collection, nestingLevel));
                return sb.ToString();
            }

            sb.Append(type.Name);

            var typeProperties = type.GetProperties();
            if (!typeProperties.Any()) return sb.ToString();

            foreach (var propertyInfo in typeProperties)
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if (excludedProperties.Contains(propertyInfo)) continue;

                var propertyValue = propertyInfo.GetValue(obj);
                var propertyValueString = GetPropertyValueString(propertyValue, propertyInfo, nestingLevel);

                sb.Append(Environment.NewLine + indentation + propertyInfo.Name + " = " + propertyValueString);
            }

            return sb.ToString();
        }

        private string GetPropertyValueString(object propertyValue, PropertyInfo propertyInfo, int nestingLevel)
        {
            var propertyValueString = string.Empty;

            if (typeCultureInfos.ContainsKey(propertyInfo.PropertyType))
            {
                var culture = typeCultureInfos[propertyInfo.PropertyType];
                propertyValueString = string.Format(culture, "{0}", propertyValue);
            }

            if (typeSerializators.ContainsKey(propertyInfo.PropertyType))
                propertyValueString =
                    typeSerializators[propertyInfo.PropertyType].DynamicInvoke(propertyValue).ToString();

            if (propertySerializators.ContainsKey(propertyInfo))
                propertyValueString =
                    propertySerializators[propertyInfo].DynamicInvoke(propertyValue).ToString();

            if (propertyValueString == string.Empty)
                propertyValueString = PrintToString(propertyValue, nestingLevel + 1);

            if (propertyTrimmedLengths.ContainsKey(propertyInfo))
                propertyValueString = TrimString(propertyValueString, propertyTrimmedLengths[propertyInfo]);

            return propertyValueString;
        }

        private string PrintToStringIEnumerable(object obj, IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var collectionCount = 1;
            var type = obj.GetType();
            var indentation = new string(Indent, nestingLevel);

            sb.AppendLine(type.Name);
            sb.AppendLine(indentation + '{');

            foreach (var item in collection)
            {
                if (collectionCount > CollectionLookDepth)
                {
                    sb.AppendLine(indentation + Indent + "...");
                    break;
                }

                var itemString = PrintToString(item, nestingLevel + 1);
                sb.AppendLine(indentation + Indent + itemString);
                collectionCount++;
            }

            sb.Append(indentation + "}");

            return sb.ToString();
        }
    }
}