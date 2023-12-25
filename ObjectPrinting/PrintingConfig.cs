using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Config configuration;
        private readonly Type[] finalTypes;

        public PrintingConfig() 
        { 
            configuration = new Config();
            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
        }

        public PrintingConfig<TOwner> Excluding<Type>()
        {
            configuration.excludedTypes.Add(typeof(Type));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<Property>(Expression<Func<TOwner, Property>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            configuration.excludedProperties.Add(propInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, configuration);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, configuration, propInfo);
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
            {
                if(configuration.cultures.TryGetValue(type, out var culture))
                    return Convert.ToString(obj, culture) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            if (obj is ICollection collection)
                return SerializeCollection(collection, nestingLevel);

            configuration.serializedObjects.Add(obj);
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder().Append(type.Name + Environment.NewLine);
            var properties = type.GetProperties().Where(propInfo => 
                !configuration.excludedProperties.Contains(propInfo) && 
                !configuration.excludedTypes.Contains(propInfo.PropertyType));

            foreach (var propertyInfo in properties)
            {
                var propertyValue = propertyInfo.GetValue(obj);
                if (configuration.serializedObjects.Contains(propertyValue))
                    continue;
                if (configuration.typeSerialization.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
                    sb.Append(identation + propertyInfo.Name + " = " + typeSerializer.DynamicInvoke(propertyValue) + Environment.NewLine);
                else if (configuration.propertiesSerialization.TryGetValue(propertyInfo, out var propSerializer))
                    sb.Append(identation + propertyInfo.Name + " = " + propSerializer.DynamicInvoke(propertyValue) + Environment.NewLine);
                else
                    sb.Append(identation + propertyInfo.Name + " = " + PrintToString(propertyValue, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string SerializeCollection(ICollection collection, int nestingLevel)
        {
            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel);

            var sb = new StringBuilder();
            foreach (var item in collection)
                sb.Append(PrintToString(item, nestingLevel + 1).Trim() + " ");

            return $"[ {sb}]" + Environment.NewLine;
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel);
            sb.Append(identation + "{" + Environment.NewLine);
            foreach (DictionaryEntry keyValuePair in dictionary)
            {
                identation = new string('\t', nestingLevel + 1);
                sb.Append(identation + "[" + PrintToString(keyValuePair.Key, nestingLevel + 1).Trim() + " - ");
                sb.Append(PrintToString(keyValuePair.Value, 0).Trim());
                sb.Append("],");
                sb.Append(Environment.NewLine);
            }

            return sb + "}" + Environment.NewLine;
        }
    }
}