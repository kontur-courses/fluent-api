using System;
using System.Collections;
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
        public Dictionary<Type, Func<object, string>> TypeSerializers { get; } = [];
        public Dictionary<Type, CultureInfo> Cultures { get; } = [];
        public Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; } = [];
        public Dictionary<PropertyInfo, int> PropertiesMaxLength { get; } = [];
        
        private readonly Type[] primitiveTypes =
        [
            typeof(int), 
            typeof(double), 
            typeof(float), 
            typeof(string),
            typeof(DateTime), 
            typeof(TimeSpan)
        ];
        private readonly HashSet<Type> excludingTypes = [];
        private readonly HashSet<PropertyInfo> excludingProperties = [];

        public PrintingConfig<TOwner> Excluding<TPropertyType>()
        {
            excludingTypes.Add(typeof(TPropertyType));
            
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            excludingProperties.Add(GetProperty(memberSelector));
            
            return this;
        }

        public TypePrintingConfig<TOwner, TPropertyType> For<TPropertyType>() => new(this);

        public PropertyPrintingConfig<TOwner, TProperty> For<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector) => 
            new(this, GetProperty(memberSelector));
        
        public string PrintToString(TOwner obj) => 
            PrintToString(obj, 0, []);
        
        private string PrintToString(object obj, int nestingLevel, Dictionary<object, int> parsedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            
            if (primitiveTypes.Contains(type))
                return obj + Environment.NewLine;

            if (parsedObjects.TryGetValue(obj, out var level))
                return $"cycled {type.Name} in level {level}" + Environment.NewLine;
            parsedObjects.Add(obj, nestingLevel);

            return obj switch
            {
                IDictionary dictionary => PrintDictionary(dictionary, nestingLevel, parsedObjects),
                IEnumerable collection => PrintCollection(collection, nestingLevel, parsedObjects),
                _ => PrintClassProperties(obj, nestingLevel, parsedObjects)
            };
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel, Dictionary<object,int> parsedObjects)
        {
            var sb = new StringBuilder();
            var nextNestingLevel = nestingLevel + 1;
            var identation = new string('\t', nextNestingLevel);
            sb.AppendLine("Dictionary");

            foreach (DictionaryEntry kvp in dictionary)
            {
                var key = kvp.Key;
                var value = kvp.Value!;
                
                sb.Append(identation + PrintToString(key, nestingLevel, parsedObjects).Trim() + 
                          " : " +
                          PrintToString(value, nestingLevel, parsedObjects));
            }
            
            return sb.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel, Dictionary<object,int> parsedObjects)
        {
            var sb = new StringBuilder();
            var nextNestingLevel = nestingLevel + 1;
            var identation = new string('\t', nextNestingLevel);

            sb.AppendLine("Collection");
            foreach (var element in collection)
                sb.Append(identation + PrintToString(element, nextNestingLevel, parsedObjects));
            
            return sb.ToString();
        }
        
        private string PrintClassProperties(object obj, int nestingLevel, Dictionary<object, int> parsedObjects)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            var nextNestingLevel = nestingLevel + 1;
            var identation = new string('\t', nextNestingLevel);
            
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingProperties.Contains(propertyInfo) || excludingTypes.Contains(propertyInfo.PropertyType))
                    continue;
                sb.Append(identation + 
                          propertyInfo.Name + 
                          " = " + 
                          PrintProperty(obj, propertyInfo, nextNestingLevel, parsedObjects));
            }
            
            return sb.ToString();
        }

        private PropertyInfo GetProperty<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException($"Expression refers to a method, not a property.");
            
            return (memberExpression.Member as PropertyInfo)!;
        }

        private string PrintProperty(
            object obj,
            PropertyInfo propertyInfo,
            int nextNestingLevel,
            Dictionary<object, int> parsedObjects)
        {
            string? result = null;
            var propertyValue = propertyInfo.GetValue(obj)!;
            
            if (PropertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
                result = propertySerializer(propertyValue);
            else if (TypeSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
                result = typeSerializer(propertyValue);
            else if (PropertiesMaxLength.TryGetValue(propertyInfo, out var maxLength))
            {
                var propertyString = (propertyValue as string)!;
                result = propertyString[..Math.Min(propertyString.Length, maxLength)];
            }
            else if (Cultures.TryGetValue(propertyInfo.PropertyType, out var culture))
                result = string.Format(culture, "{0}", propertyValue);
            
            return result == null ? 
                PrintToString(propertyValue, nextNestingLevel, parsedObjects) : 
                result + Environment.NewLine;
        }
    }
}