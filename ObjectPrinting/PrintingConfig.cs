using System;
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
        private readonly IEnumerable<Type> baseTypes = new List<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly List<string> excludingProperty = new List<string>();
        private readonly List<Type> excludingTypes = new List<Type>();
        private readonly Dictionary<Type, Delegate> specialTypeSerializationFunction = new Dictionary<Type, Delegate>();

        private readonly Dictionary<Type, CultureInfo> specialTypeSerializationCulture =
            new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<string, int> cutProperty = new Dictionary<string, int>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = GetPropertyName(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = GetPropertyName(memberSelector);
            excludingProperty.Add(propertyName);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        internal void AddTypeSerialization(Type type, Delegate function)
        {
            specialTypeSerializationFunction[type] = function;
        }

        internal void AddTypeSerializationCulture(Type type, CultureInfo culture)
        {
            specialTypeSerializationCulture[type] = culture;
        }

        internal void AddCutProperty(string propertyName, int length)
        {
            cutProperty[propertyName] = length;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        private string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = memberSelector.Body as MemberExpression;
            var propertyName = memberExpression?.Member.Name;
            return propertyName;
        }
        
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (baseTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var objectType = obj.GetType();
            var stringBuilder = new StringBuilder(objectType.Name + Environment.NewLine);

            foreach (var propertyInfo in objectType.GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;
                if (excludingTypes.Contains(propertyType)) continue;
                if (excludingProperty.Contains(propertyName)) continue;

                stringBuilder.Append(indentation + propertyName + " = " +
                                     PrintToString(GetProperty(propertyInfo, obj),
                                         nestingLevel + 1));
            }

            return stringBuilder.ToString();
        }

        private object GetProperty(PropertyInfo propertyInfo, object obj)
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyValue = propertyInfo.GetValue(obj);
            var propertyName = propertyInfo.Name;

            if (specialTypeSerializationFunction.TryGetValue(propertyType, out var serializeFunction))
                return serializeFunction?.DynamicInvoke(propertyValue);

            if (specialTypeSerializationCulture.TryGetValue(propertyType, out var serializeCultureInfo))
                return ((IFormattable) propertyValue).ToString("g", serializeCultureInfo);

            if (cutProperty.TryGetValue(propertyName, out var maxLen))
            {
                var len = propertyValue.ToString().Length > maxLen ? maxLen : propertyValue.ToString().Length;
                return propertyValue.ToString().Substring(0, len);
            }

            return propertyInfo.GetValue(obj);
        }
    }
}