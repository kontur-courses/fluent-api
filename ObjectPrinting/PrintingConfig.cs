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
        public Dictionary<Type, CultureInfo> Cultures = new Dictionary<Type, CultureInfo>();
        public readonly Dictionary<Type, Delegate> TypePrintingConfig = new Dictionary<Type, Delegate>();
        public readonly Dictionary<PropertyInfo, Delegate> PropertyToCut = new Dictionary<PropertyInfo, Delegate>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private Dictionary<PropertyInfo, Delegate> propertyPrintingConfig = new Dictionary<PropertyInfo, Delegate>();
        private readonly TOwner value;

        public PrintingConfig(TOwner value)
        {
            this.value = value;
        }
        
        public PrintingConfig()
        {
            value = default;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            var property = (expression.Body as MemberExpression).Member;
            excludedProperties.Add(property as PropertyInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this, typeof(T));
        }
        
        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> expression)
        {
            var property = (expression.Body as MemberExpression)?.Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, property);
        }

        public string PrintToString()
        {
            return PrintToString(value);
        }

        public string PrintToString(int nestingLevel)
        {
            return PrintToString(value, nestingLevel);
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

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) ||
                    excludedProperties.Contains(propertyInfo))
                    continue;
                var value = propertyInfo.GetValue(obj);
                if (TypePrintingConfig.ContainsKey(propertyInfo.PropertyType))
                    value = TypePrintingConfig[propertyInfo.PropertyType].DynamicInvoke(value);
                if (propertyPrintingConfig.ContainsKey(propertyInfo))
                    value = propertyPrintingConfig[propertyInfo].DynamicInvoke(value);
                if(PropertyToCut.ContainsKey(propertyInfo))
                    value = PropertyToCut[propertyInfo].DynamicInvoke(value);
                sb.Append(indentation).Append(propertyInfo.Name).Append(" = ")
                    .Append(PrintToString(value, nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}