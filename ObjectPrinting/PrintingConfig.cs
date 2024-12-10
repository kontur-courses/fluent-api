using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes = [
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];
        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<PropertyInfo> excludedProperties = [];
        private readonly Dictionary<PropertyInfo, Func<object, string>> customPropertySerializers = [];
        private readonly Dictionary<Type, Func<object, string>> customTypeSerializers = [];
        private readonly HashSet<object> visitedObjects = [];

        public IPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(
                this, 
                customTypeSerializers);
        }

        public IPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberInfo)
                throw new ArgumentException($"Expression '{memberSelector}' refers to a method, not a property.");
            return new PropertyPrintingConfig<TOwner, TPropType>(
                this,
                customPropertySerializers,
                memberInfo.Member as PropertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberInfo)
                throw new ArgumentException($"Expression '{memberSelector}' refers to a method, not a property.");
            excludedProperties.Add(memberInfo.Member as PropertyInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (visitedObjects.Contains(obj))
                return "Cycling reference!";
 
            if (FinalTypes.Contains(obj.GetType()))
                return obj.ToString();

            return obj switch
            {
                IDictionary dictionary => PrintToStringIDictionary(dictionary, nestingLevel),
                IEnumerable enumerable => PrintToStringIEnumerable(enumerable, nestingLevel),
                _ => PrintToStringObject(obj, nestingLevel)
            };
        }
        
        private bool TryUseCustomSerializer(object value, PropertyInfo propertyInfo, out string? str)
        {
            str = null;
            
            if (customPropertySerializers.TryGetValue(propertyInfo, out var lambda) || 
                customTypeSerializers.TryGetValue(propertyInfo.PropertyType, out lambda))
            {
                str = lambda(value);
                return true;
            }
            
            return false;
        }

        private IEnumerable<PropertyInfo> GetIncludedProperties(Type type)
            => type.GetProperties().Where(p => !IsExcluded(p));
        
        private bool IsExcluded(PropertyInfo propertyInfo) 
            => excludedProperties.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);

        private string PrintToStringObject(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.Append(type.Name);
            visitedObjects.Add(obj);
            
            foreach (var propertyInfo in GetIncludedProperties(type))
            {
                var value = propertyInfo.GetValue(obj);
                var serializedStr = TryUseCustomSerializer(value, propertyInfo, out var str)
                    ? str
                    : PrintToString(value, nestingLevel + 1);
                builder.Append('\n' + indentation + propertyInfo.Name + " = " + serializedStr);
            }
            
            visitedObjects.Remove(obj);
            return builder.ToString();
        }

        private string PrintToStringIDictionary(IDictionary dictionary, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            builder.Append(dictionary.GetType().Name);
            visitedObjects.Add(dictionary);

            foreach (DictionaryEntry item in dictionary)
                builder.Append('\n' + indentation + PrintToString(item.Key, nestingLevel + 1) + " = " +
                               PrintToString(item.Value, nestingLevel + 1));

            visitedObjects.Remove(dictionary);
            return builder.ToString();
        }
        
        private string PrintToStringIEnumerable(IEnumerable collection, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var index = 0;
            builder.Append(collection.GetType().Name);
            visitedObjects.Add(collection);

            foreach (var item in collection)
                builder.Append('\n' + indentation + $"{index++} = " + PrintToString(item, nestingLevel + 1));

            visitedObjects.Remove(collection);
            return builder.ToString();
        }
    }
}