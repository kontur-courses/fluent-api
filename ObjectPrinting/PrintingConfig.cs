using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        protected readonly HashSet<Type> ExcludedTypes = [];
        protected readonly HashSet<string> ExcludedProperties = [];
        protected readonly Dictionary<string, Func<object, string>> PropertySerializers = [];
        protected readonly Dictionary<Type, Func<object, string>> PropertyTypeSerializers = [];
        private readonly HashSet<Type> finalTypes = new HashSet<Type>()
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };

        private HashSet<object> visited = [];

        public PrintingConfig() { }

        protected PrintingConfig(PrintingConfig<TOwner> clone)
        {
            PropertySerializers = clone.PropertySerializers;
            PropertyTypeSerializers = clone.PropertyTypeSerializers;
            ExcludedProperties = clone.ExcludedProperties;
            ExcludedTypes = clone.ExcludedTypes;
        }

        public PrintingConfig<TOwner> Exclude<TForExcluding>()
        {
            ExcludedTypes.Add(typeof(TForExcluding));
            return this;
        }

        internal PrintingConfig<TOwner> RefineSerializer<TProperty>(Func<string, string> serializer)
        {
          PropertyTypeSerializers[typeof(TProperty)] =
                PropertyTypeSerializers.TryGetValue(typeof(TProperty), out var startSerializer) 
                ? (object x) => serializer(startSerializer((TProperty)x))
                : (object x) => serializer(((TProperty)x).ToString());
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> getProperty)
        {
            var propName = GetPropertyName(getProperty);
            ExcludedProperties.Add(propName);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProperty> WithSerializer<TProperty>(
            Expression<Func<TOwner, TProperty>> getProperty,
            Func<TProperty, string> serializer)
        {
            var propName = GetPropertyName(getProperty);
            PropertySerializers.Add(propName, (object x) => serializer((TProperty)x));
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> ConfigurePropertyConfig<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PrintingConfig<TOwner> WithSerializer<TProperty>(Func<TProperty, string> serializer)
        {
            PropertyTypeSerializers[typeof(TProperty)] = (object x) => serializer((TProperty)x);
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

        protected string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (visited.Contains(obj))
                return "Cycle Reference" + Environment.NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name.Split('`')[0]);
            visited.Add(obj);

            if (obj is ICollection) return PrintToStringCollection(obj as ICollection, nestingLevel, sb);

            foreach (var memberInfo in type.GetPublicFieldsAndProperties())
            {
                sb.Append(PropertyToString(memberInfo, obj, nestingLevel));
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
            MemberInfo info,
            object obj,
            int nestingLevel)
        {
            var propType = info.GetMemberType();
            var memberValue = info.GetValue(obj) ?? "null";

            if (!ExcludedTypes.Contains(propType) && !ExcludedProperties.Contains(info.Name))
            {
                Func<object, string> serializer;
                string propValueString;
                if (PropertySerializers.TryGetValue(info.Name, out serializer) ||
                    PropertyTypeSerializers.TryGetValue(propType, out serializer))
                {
                    propValueString = serializer(memberValue);
                }
                else
                {
                    propValueString = (finalTypes.Contains(propType) || memberValue == "null")
                        ? memberValue.ToString()
                        : PrintToString(memberValue, nestingLevel + 1);
                }
                return GetBeautyPropertyString(info.Name, propValueString, nestingLevel);
            }
            return "";
        }

        private string GetBeautyPropertyString(string name, string propValueString, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var stringStart = identation + name + " = ";
            var stringEnd = Environment.NewLine;
            return stringStart + propValueString + stringEnd;
        }
    }
}