using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly SerializationSettings _serializationSettings;

        public PrintingConfig()
        {
            _serializationSettings = new SerializationSettings();
        }

        SerializationSettings IPrintingConfig<TOwner>.SerializationSettings => _serializationSettings;

        public string PrintToString(TOwner obj)
        {
            return PrintObjectToString(obj, 0);
        }

        public string PrintToString(IList<TOwner> lsit)
        {
            return PrintEnumerableToString(lsit);
        }

        public string PrintToString(TOwner[] array)
        {
            return PrintEnumerableToString(array);
        }

        public string PrintToString<TValue>(Dictionary<TOwner, TValue> dict)
        {
            return PrintDictionaryToString(dict);
        }

        public string PrintToString<TKey>(Dictionary<TKey, TOwner> dict)
        {
            return PrintDictionaryToString(dict);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _serializationSettings.AddTypeToExclude(typeof(TPropType));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            var memberBody = (MemberExpression)property.Body;
            var propertyInfo = memberBody.Member as PropertyInfo;
            _serializationSettings.AddPropertyToExclude(propertyInfo);

            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> printProperty)
        {
            var memberBody = (MemberExpression)printProperty.Body;
            var propertyInfo = memberBody.Member as PropertyInfo;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        private string PrintObjectToString(object obj, int nestingLevel)
        {
            _serializationSettings.GetSerializedObjects().Clear();
            var serializer = new Serializer(_serializationSettings);

            return serializer.SerializeObject(obj, nestingLevel);
        }

        private string PrintEnumerableToString(IEnumerable<TOwner> enumerable)
        {
            var enumerableInString = new StringBuilder();

            foreach (var item in enumerable)
                enumerableInString.Append(PrintObjectToString(item, 0));

            return enumerableInString.ToString();
        }

        private string PrintDictionaryToString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            var dictInString = new StringBuilder();

            foreach (var pair in dictionary)
                dictInString.AppendLine(PrintObjectToString(pair.Key, 0) + " : " + PrintObjectToString(pair.Value, 0));

            return dictInString.ToString();
        }
    }
}