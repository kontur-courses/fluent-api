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
        private static readonly HashSet<Type> FinalTypes = new HashSet<Type>()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedProperty = new HashSet<MemberInfo>();

        private readonly Dictionary<MemberInfo, Func<object,string>> customPropertySerialize =
            new Dictionary<MemberInfo, Func<object,string>>();

        private readonly Dictionary<Type, Func<object,string>> customTypeSerializers = new Dictionary<Type, Func<object,string>>();
        private readonly HashSet<object> visitedObjects = new HashSet<object>();


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var builder = new StringBuilder();
            //TODO apply configurations
            if (obj == null)
                return "null";

            if (visitedObjects.Contains(obj))
            {
                return $"Cycling references";
            }

            //visitedObjects.Add(obj);

            if (FinalTypes.Contains(obj.GetType()))
                return obj.ToString();

            if (obj is IDictionary dictionary)
            {
                return PrintToStringIDictionary(dictionary, nestingLevel + 1);
            }

            if (obj is IEnumerable collection)
            {
                return PrintToStringIEnumerable(collection, nestingLevel + 1);
            }

            return PrintToStringProperties(obj, nestingLevel + 1);
        }

        private string PrintToStringProperties(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var builder = new StringBuilder();
            var objType = obj.GetType();
            builder.Append($"{objType.Name}");
            visitedObjects.Add(obj);
            foreach (var propertyInfo in GetIncludedProperties(objType))
            {
                var value = propertyInfo.GetValue(obj);
                var customSerializeStr = TryUseCustomSerializer(value, propertyInfo, out var str)
                    ? str
                    : PrintToString(value, nestingLevel + 1);
                builder.Append('\n' + identation + propertyInfo.Name + " = " + customSerializeStr);
            }
            visitedObjects.Remove(obj);
            return builder.ToString();
        }

        private string PrintToStringIEnumerable(IEnumerable collection, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var builder = new StringBuilder();
            visitedObjects.Add(collection);
            foreach (var item in collection)
            {
                builder.Append('\n' + identation + PrintToString(item, nestingLevel + 1));
            }

            visitedObjects.Remove(collection);
            return builder.ToString();
        }

        private string PrintToStringIDictionary(IDictionary dictionary, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var builder = new StringBuilder();

            visitedObjects.Add(dictionary);

            foreach (var key in dictionary.Keys)
            {
                builder.Append('\n' + identation + PrintToString(key, nestingLevel + 1) + " = " +
                               PrintToString(dictionary[key], nestingLevel + 1));
            }

            visitedObjects.Remove(dictionary);
            return builder.ToString();
        }

        private bool TryUseCustomSerializer(object value, PropertyInfo propertyInfo, out string str)
        {
            str = null;
            if (customPropertySerialize.ContainsKey(propertyInfo))
            {
                str = (string)customPropertySerialize[propertyInfo].DynamicInvoke(value);
                return true;
            }

            var type = propertyInfo.PropertyType;
            if (customTypeSerializers.ContainsKey(propertyInfo.PropertyType))
            {
                str = (string)customTypeSerializers[propertyInfo.PropertyType].DynamicInvoke(value);
                return true;
            }

            return false;
        }

        private IEnumerable<PropertyInfo> GetIncludedProperties(Type type)
        {
            return type.GetRuntimeProperties().Where(p => !IsExclude(p));
        }

        private bool IsExclude(PropertyInfo member)
        {
            return excludedTypes.Contains(member.PropertyType) || excludedProperty.Contains(member);
        }

        public PrintingConfig<TOwner> Exclude<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> func)
        {
            var memberInfo = func.Body as MemberExpression;
            if (memberInfo == null)
                throw new ArgumentException();
            excludedProperty.Add(memberInfo.Member);
            return this;
        }


        public PrintingConfig<TOwner> SetCustomTypeSerializer<TType>(Func<TType, string> serializer)
        {
            customTypeSerializers[typeof(TType)] = obj =>serializer((TType)obj);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProperty> SerializeByProperty<TProperty>(
            Expression<Func<TOwner, TProperty>> func)
        {
            var memberInfo = func.Body as MemberExpression;
            if (memberInfo == null)
                throw new ArgumentException();
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberInfo.Member);
        }

        public PrintingConfig<TOwner> SetCustomPropertySerializer<TProperty>(MemberInfo property,
            Func<TProperty, string> serializer)
        {
            customPropertySerialize[property] = obj =>serializer((TProperty)obj);
            return this;
        }

        public PrintingConfig<TOwner> SetCulture<TType>(string format, CultureInfo culture)
            where TType : IFormattable

        {
            return SetCustomTypeSerializer((TType obj) => obj.ToString(format, culture));
        }
    }
}