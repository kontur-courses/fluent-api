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
        private readonly Dictionary<object, object> serializes = new Dictionary<object, object>();
        private readonly List<object> excludedObjects = new List<object>();

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            if (!excludedObjects.Contains(typeof(TProperty)))
                excludedObjects.Add(typeof(TProperty));
            
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            var propertyName = memberExpression.Member.Name;
            
            if (!excludedObjects.Contains(propertyName))
                excludedObjects.Add(propertyName);
            
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this, null, serializes);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> expr)
        {
            var memberExpression = (MemberExpression)expr.Body;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, (PropertyInfo)memberExpression.Member, serializes);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(type.Name);
            
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedObjects.Contains(propertyInfo.Name))
                    continue;
                
                if (excludedObjects.Contains(propertyInfo.PropertyType))
                    continue;

                if (serializes.ContainsKey(propertyInfo.Name))
                {
                    if (serializes[propertyInfo.Name] is Delegate func)
                        stringBuilder.Append(func.DynamicInvoke(propertyInfo.GetValue(obj)));
                    continue;
                }

                if (serializes.ContainsKey(propertyInfo.PropertyType))
                {
                    if (serializes[propertyInfo.PropertyType] is CultureInfo cultureInfo)
                    {
                        var nextString = PrintToString(Convert.ToString(propertyInfo.GetValue(obj), cultureInfo), nestingLevel + 1);
                        stringBuilder.Append(identation + propertyInfo.Name + " = " + nextString);
                    }
                    
                    if (serializes[propertyInfo.PropertyType] is Delegate func)
                        stringBuilder.Append(func.DynamicInvoke(propertyInfo.GetValue(obj)));
                    continue;
                }
                
                stringBuilder.Append(identation + propertyInfo.Name + " = " + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
            
            return stringBuilder.ToString();
        }
    }
}