using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ArrayPrintingConfig<T> : PrintingConfig<T[]>
    {
        public override string PrintToString(T[] obj)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            for (var i = 0; i < obj.Length; i++)
            {
                sb.Append($"[{i}]" + base.PrintToString(obj[i], 1));
            }
            return sb.ToString();
        }
    }
    public class PrintingConfig<TOwner>
    {
        protected HashSet<Type> excludedTypes = [];
        protected HashSet<string> excludedProperties = [];
        protected Dictionary<string, Func<object, string>> propertySerializers = [];
        protected Dictionary<Type, Func<object, string>> propertyTypeSerializers = [];
        private HashSet<Type> finalTypes = new HashSet<Type>()
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

        private HashSet<object> visited = [];

        public PrintingConfig() {}

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

        public virtual string PrintToString(TOwner obj)
        {
            visited = new();
            return PrintToString(obj, 0);
        }

        protected string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (visited.Contains(obj)) 
                return "Cycle Reference" + Environment.NewLine;

            
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name.Split('`')[0]);
            visited.Add(obj);

            if (obj is ICollection) return PrintToStringCollection(obj as ICollection, nestingLevel, sb);

            foreach (var propertyInfo in type.GetProperties(flags))
            {
                var propType = propertyInfo.PropertyType;
                var propName = propertyInfo.Name;
                var value = propertyInfo.GetValue(obj) ?? "null";
                sb.Append(PropertyToString(propName, propType, value, nestingLevel));
            }
            foreach (var propertyInfo in type.GetFields(flags))
            {
                var propType = propertyInfo.FieldType;
                var propName = propertyInfo.Name;
                var value = propertyInfo.GetValue(obj) ?? "null";
                sb.Append(PropertyToString(propName, propType, value, nestingLevel));
            }
            return sb.ToString().Trim();
        }

        private string PrintToStringCollection(ICollection obj, int nestingLevel, StringBuilder sb)
        {
            var id = 0;
            foreach (var item in obj)
            {
                sb.Append($"\t[{id}] = " + PrintToString(item, nestingLevel + 1) + Environment.NewLine);
                id++;
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

            if (!excludedTypes.Contains(propType) && !excludedProperties.Contains(propName))
            {
                Func<object, string> serializer;
                var stringStart = identation + propName + " = ";
                var stringEnd = Environment.NewLine;
                string propValueString;
                if (propertySerializers.TryGetValue(propName, out serializer) ||
                    propertyTypeSerializers.TryGetValue(propType, out serializer))
                {
                    propValueString = serializer(value);
                }
                else
                {
                    propValueString = (finalTypes.Contains(propType) || value == "null") 
                        ? value.ToString()
                        : PrintToString(value, nestingLevel + 1);
                }
                return stringStart + propValueString + stringEnd;
            }
            return "";
        }
    }
}