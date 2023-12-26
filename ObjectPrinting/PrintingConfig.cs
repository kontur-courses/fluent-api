using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        internal readonly Dictionary<Type, Func<object, string>> TypeSerializers = new Dictionary<Type, Func<object, string>>();
        internal readonly Dictionary<PropertyInfo, Func<object, string>> PropertySerializers = new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly HashSet<object> serializedObjects = new HashSet<object>();

        public PrintingConfig<TOwner> Exclude<TProp>()
        {
            excludedTypes.Add(typeof(TProp));
            return this;
        }
        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> exclude)
        {
            var propertyInfo = ((MemberExpression)exclude.Body).Member as PropertyInfo;
            excludedProperties.Add(propertyInfo);
            return this;
        }

        public TypePrintingConfig<TOwner,TProperty> Serialize<TProperty>()
        {
            return new TypePrintingConfig<TOwner,TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProp> Serialize<TProp>(Expression<Func<TOwner, TProp>> customSerialize)
        {
            var propertyInfo = ((MemberExpression)customSerialize.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TProp>(this, propertyInfo);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string DictionarySerialize(PropertyInfo propertyInfo, object obj)
        {
            var dictionaryValue = propertyInfo.GetValue(obj) as IDictionary;
            if (dictionaryValue == null)
            {
                return string.Empty;
            }

            var keys = dictionaryValue.Keys.OfType<object>().Select(item => item.ToString()).ToArray();
            var values = dictionaryValue.Values.OfType<object>().Select(item => item.ToString()).ToArray();
            var pair = keys.Zip(values).Select(pair => string.Format("({0}, {1})", pair.First, pair.Second));

            return $"{propertyInfo.Name} = {{ " + string.Join(", ", pair) + " }";
        }

        private string IEnumerableSerialize(PropertyInfo propertyInfo, object obj)
        {
            var listValue = propertyInfo.GetValue(obj) as IEnumerable;
            if (listValue == null)
            {
                return string.Empty;
            }

            var items = listValue.OfType<object>().Select(item => item.ToString()).ToArray();
            return $"{propertyInfo.Name} = {{ " + string.Join(", ", items) + " }";
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (serializedObjects.Contains(obj))
                return obj.GetType().GetTypeInfo().Name + Environment.NewLine;

            serializedObjects.Add(obj);

            var identation = new string('\t', nestingLevel + 1);

            if(nestingLevel > 5)
                return identation;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (type.IsValueType || obj is string)
            {
                if (TypeSerializers.ContainsKey(type))
                {

                    return TypeSerializers[type](obj) + Environment.NewLine;
                }
                return obj + Environment.NewLine;
            }

            if (type.IsValueType && TypeSerializers.ContainsKey(type))
            {
                sb.Append(TypeSerializers[type](obj) + Environment.NewLine);
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo))
                    continue;
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (PropertySerializers.ContainsKey(propertyInfo))
                {
                    sb.Append(identation + propertyInfo.Name + " = " + PropertySerializers[propertyInfo](propertyInfo.GetValue(obj)) + Environment.NewLine);
                }
                else if (propertyInfo.PropertyType.IsArray
                    || (propertyInfo.PropertyType.IsGenericType
                    && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    sb.Append(identation + IEnumerableSerialize(propertyInfo, obj) + Environment.NewLine);
                }
                else if (propertyInfo.PropertyType.IsGenericType
                    && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    sb.Append(identation + DictionarySerialize(propertyInfo, obj) + Environment.NewLine);
                }
                else
                {
                    sb.Append(identation + propertyInfo.Name + " = " + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
                }
            }
            return sb.ToString();
        }
    }
}