using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private ImmutableDictionary<Type, Delegate> typeSerializationFormats = ImmutableDictionary<Type, Delegate>.Empty;
        private ImmutableDictionary<PropertyInfo, Delegate> propertySerializationFormats = ImmutableDictionary<PropertyInfo, Delegate>.Empty;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, property);
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

                sb.Append(identation);

                if (propertySerializationFormats.ContainsKey(propertyInfo))
                    sb.Append(propertySerializationFormats[propertyInfo]
                                  .DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                else if (typeSerializationFormats.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(typeSerializationFormats[propertyInfo.PropertyType]
                                  .DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                else
                    sb.Append(propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        void IPrintingConfig<TOwner>.AddPropertySerializationFormat(PropertyInfo property, Delegate format)
        {
            propertySerializationFormats = propertySerializationFormats.Add(property, format);
        }

        void IPrintingConfig<TOwner>.AddTypeSerializationFormat(Type type, Delegate format)
        {
            typeSerializationFormats = typeSerializationFormats.Add(type, format);
        }
    }
}