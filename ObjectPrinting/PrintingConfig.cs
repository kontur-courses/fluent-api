using FluentAssertions.Equivalency;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        protected HashSet<Type> excludedTypes = new();
        protected HashSet<string> excludedProperties = new();
        protected Dictionary<string, Func<object, string>> propertySerializers = new();
        protected Dictionary<Type, Func<object, string>> propertyTypeSerializers = new();

        private HashSet<object> visited = new();

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
        }

        protected PrintingConfig(PrintingConfig<TOwner> clone)
        {
            propertySerializers = clone.propertySerializers;
            propertyTypeSerializers = clone.propertyTypeSerializers;
            excludedProperties = clone.excludedProperties;
            excludedTypes = clone.excludedTypes;
        }

        public PrintingConfig<TOwner> Exclude<TForExcluding>()
        {
            excludedTypes.Add(typeof(TForExcluding));
            return this;
        }

        internal PrintingConfig<TOwner> RefineSerializer<TProperty>(Func<string, string> serializer)
        {
            if (propertyTypeSerializers.TryGetValue(typeof(TProperty), out var startSerializer))
            {
                propertyTypeSerializers[typeof(TProperty)] = (object x) => serializer(startSerializer((TProperty)x));
            }
            else
            {
                propertyTypeSerializers[typeof(TProperty)] = (object x) => serializer(((TProperty)x).ToString());
            }
            
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> getProperty)
        {
            var propName = GetPropertyName(getProperty);
            excludedProperties.Add(propName);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProperty> WithSerializer<TProperty>(
            Expression<Func<TOwner, TProperty>> getProperty,
            Func<TProperty, string> serializer)
        {
            var propName = GetPropertyName(getProperty);
            propertySerializers.Add(propName, (object x) => serializer((TProperty)x));
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Func<TOwner, TProperty> get)
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PrintingConfig<TOwner> WithSerializer<TProperty>(Func<TProperty, string> serializer)
        { 
            propertyTypeSerializers[typeof(TProperty)] = (object x) => serializer((TProperty)x);
            return this;
        }

        private static string GetPropertyName<T, TValue>(Expression<Func<T, TValue>> property)
        {
            var memberExpress = property.Body as MemberExpression;
            return memberExpress.Member.Name;
        }


        public string PrintToString(TOwner obj)
        {
            visited = new();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (visited.Contains(obj)) 
                return "Cycle Reference" + Environment.NewLine;
          
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            visited.Add(obj);
            foreach (var propertyInfo in type.GetProperties(flags))
            {
                var propName = propertyInfo.Name;
                var propType = propertyInfo.PropertyType;
                var value = propertyInfo.GetValue(obj) ?? "null";
                sb.Append(PropertyToString(propName, propType, value, nestingLevel));
            }
            foreach (var propertyInfo in type.GetFields(flags))
            {
                var propName = propertyInfo.Name;
                var propType = propertyInfo.FieldType;
                var value = propertyInfo.GetValue(obj) ?? "null";
                sb.Append(PropertyToString(propName, propType, value, nestingLevel));
            }
            return sb.ToString().Trim();
        }

        private string PropertyToString(
            string? propName,
            Type? propType,
            object value,
            int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

            if (!excludedTypes.Contains(propType) && !excludedProperties.Contains(propName))
            {
                Func<object, string> serializer;
                if (propertySerializers.TryGetValue(propName, out serializer) ||
                    propertyTypeSerializers.TryGetValue(propType, out serializer))
                {
                    return identation + propName + " = " + serializer(value) + Environment.NewLine;
                }
                if (finalTypes.Contains(propType) || value == "null")
                {
                    return identation + propName + " = " + value.ToString() + Environment.NewLine;
                }

                return identation + propName + " = " + PrintToString(value, nestingLevel + 1) + Environment.NewLine;
            }
            return "";
        }
    }
}