using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> IPrintingConfig.WaysToSerializeTypes => waysToSerializeTypes; 
        Dictionary<Type, CultureInfo> IPrintingConfig.TypesCultures => typesCultures;
        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig.WaysToSerializeProperties =>
            waysToSerializeProperties; 
        Dictionary<PropertyInfo, int> IPrintingConfig.MaxLengthsOfProperties => maxLengthsOfProperties;

        private readonly HashSet<Type> typesToIgnore;
        private readonly Dictionary<Type, Func<object, string>> waysToSerializeTypes;
        private readonly Dictionary<Type, CultureInfo> typesCultures;
        private readonly Dictionary<PropertyInfo, Func<object, string>> waysToSerializeProperties;
        private readonly Dictionary<PropertyInfo, int> maxLengthsOfProperties;
        private readonly HashSet<PropertyInfo> propertiesToIgnore;

        public PrintingConfig()
        {
            typesToIgnore = new HashSet<Type>();
            waysToSerializeTypes = new Dictionary<Type, Func<object, string>>();
            typesCultures = new Dictionary<Type, CultureInfo>();
            waysToSerializeProperties = new Dictionary<PropertyInfo, Func<object, string>>();
            maxLengthsOfProperties = new Dictionary<PropertyInfo, int>();
            propertiesToIgnore = new HashSet<PropertyInfo>();
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public ConcretePropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            return new ConcretePropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            propertiesToIgnore.Add(propertyInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToIgnore.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (waysToSerializeTypes.ContainsKey(type))
                return waysToSerializeTypes[type](obj) + Environment.NewLine;

            if (typesCultures.ContainsKey(type))
                return string.Format(typesCultures[type], "{0}", obj) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties()
                .Where(p => !typesToIgnore.Contains(p.PropertyType) && !propertiesToIgnore.Contains(p)))
            {
                var propertyValue = waysToSerializeProperties.ContainsKey(propertyInfo)
                    ? waysToSerializeProperties[propertyInfo](propertyInfo.GetValue(obj))
                    : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

                if (maxLengthsOfProperties.ContainsKey(propertyInfo))
                    propertyValue = propertyValue.Truncate(maxLengthsOfProperties[propertyInfo]);

                sb.Append(indentation + propertyInfo.Name + " = " + propertyValue);
            }
            return sb.ToString();
        }
    }
}