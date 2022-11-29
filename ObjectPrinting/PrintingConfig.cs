using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly Dictionary<string, Delegate> PropertySerializes = new Dictionary<string, Delegate>();
        public readonly Dictionary<Type, Delegate> TypeSerializes = new Dictionary<Type, Delegate>();
        public readonly Dictionary<Type, CultureInfo> Cultures = new Dictionary<Type, CultureInfo>();

        private readonly List<object> excludedProperties = new List<object>();
        private readonly List<Type> excludedTypes = new List<Type>();

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            excludedTypes.Add(typeof(TProperty));
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            excludedProperties.Add(memberExpression.Member.Name);
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberExpression.Member);
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
            
            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                return obj + Environment.NewLine;
            }
                

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                
                if (excludedProperties.Contains(propertyInfo.Name))
                    continue;

                if (PropertySerializes.ContainsKey(propertyInfo.Name))
                {
                    sb.Append(PropertySerializes[propertyInfo.Name].DynamicInvoke(propertyInfo.GetValue(obj)));
                    continue;
                }
                if (TypeSerializes.ContainsKey(propertyInfo.PropertyType))
                {
                    sb.Append(TypeSerializes[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)));
                    continue;
                }
                
                if (Cultures.ContainsKey(propertyInfo.PropertyType))
                {
                    var nextString = PrintToString(Convert.ToString(propertyInfo.GetValue(obj), Cultures[propertyInfo.PropertyType]), nestingLevel + 1);
                    sb.Append(identation + propertyInfo.Name + " = " + nextString);
                    continue;
                }
                
                sb.Append(identation + propertyInfo.Name + " = " + PrintToString(obj, nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}