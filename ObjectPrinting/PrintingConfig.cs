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
        private HashSet<Type> excludedTypes;
        private HashSet<PropertyInfo> exludedProperties;
        internal Dictionary<Type, CultureInfo> cultures;
        internal Dictionary<Type, Delegate> typePropertyConfigs;
        internal Dictionary<string, Delegate> namePropertyConfigs;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            exludedProperties = new HashSet<PropertyInfo>();
            cultures = new Dictionary<Type, CultureInfo>();
            typePropertyConfigs = new Dictionary<Type, Delegate>();
            namePropertyConfigs = new Dictionary<string, Delegate>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            var func = memberSelector.Compile();
            namePropertyConfigs[propInfo.Name] = func;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            exludedProperties.Add(propInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
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

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
            {
                if (!cultures.ContainsKey(objType) || !(obj is IFormattable formattable))
                    return obj + Environment.NewLine;
                var culture = cultures[objType];
                return formattable.ToString(null, culture) + Environment.NewLine;
            }


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (exludedProperties.Contains(propertyInfo))
                    continue;
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                var value = propertyInfo.GetValue(obj);
                if (typePropertyConfigs.ContainsKey(propertyInfo.PropertyType))
                    value = typePropertyConfigs[propertyInfo.PropertyType].DynamicInvoke(value);
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(value,
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}