using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private HashSet<Type> excludedTypes = new HashSet<Type>();
        private HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private Dictionary<Type, Delegate> typeSerializationFormats = new Dictionary<Type, Delegate>();
        private Dictionary<PropertyInfo, Delegate> propertySerializationFormats = new Dictionary<PropertyInfo, Delegate>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            if (!(property.Body is MemberExpression body))
                throw new ArgumentException();
            var propertyName = body.Member.Name;
            excludedProperties.Add(typeof(TOwner).GetProperty(propertyName));
            return this;
        }
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        public PrintingConfig<TOwner> Exclude<TPropType>()
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

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo) ||
                    excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (propertySerializationFormats.ContainsKey(propertyInfo))
                {
                    sb.Append(identation + propertySerializationFormats[propertyInfo].DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                if (typeSerializationFormats.ContainsKey(propertyInfo.PropertyType))
                {
                    sb.Append(identation + typeSerializationFormats[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypeSerializationFormats => typeSerializationFormats;
        Dictionary<PropertyInfo, Delegate> IPrintingConfig<TOwner>.PropertySerializationFormats => propertySerializationFormats;
    }
}