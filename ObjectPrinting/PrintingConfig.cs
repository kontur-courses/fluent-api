using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TType>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<string> excludedNames = new List<string>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        protected internal readonly Dictionary<Type, CultureInfo> CultureInfos = new Dictionary<Type, CultureInfo>();

        protected internal readonly Dictionary<Type, Func<object, string>> SpecialSerializationForTypes =
            new Dictionary<Type, Func<object, string>>();

        protected internal readonly Dictionary<string, Func<object, string>> SpecialSerializationForNames =
            new Dictionary<string, Func<object, string>>();


        public PrintingConfig<TType> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TType> Excluding<TPropType>(Expression<Func<TType, TPropType>> memberSelector)
        {
            var returnedPropertyName = ((MemberExpression) memberSelector.Body).Member.Name;
            excludedNames.Add(returnedPropertyName);
            return this;
        }

        public PropertyPrintingConfig<TType, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TType, TPropType>(this);
        }

        public PropertyPrintingConfig<TType, TPropType> Printing<TPropType>(
            Expression<Func<TType, TPropType>> memberSelector)
        {
            var returnedPropertyName = ((MemberExpression) memberSelector.Body).Member.Name;
            return new PropertyPrintingConfig<TType, TPropType>(this, returnedPropertyName);
        }

        public string PrintToString(TType obj)
        {
            return obj is ICollection collection ? PrintCollection(collection, 0) : PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";
            if (nestingLevel > 10)
                throw new ArgumentException("Cycling reference");
            var sb = new StringBuilder();
            var shift = new string('\t', nestingLevel + 1);
            sb.Append(obj.GetType().Name + Environment.NewLine);
            if (CultureInfos.ContainsKey(obj.GetType()))
                CultureInfo.CurrentCulture = CultureInfos[obj.GetType()];
            if (SpecialSerializationForTypes.ContainsKey(obj.GetType()))
                return SpecialSerializationForTypes[obj.GetType()](obj);
            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();

            foreach (var property in obj.GetType().GetProperties())
            {
                if (excludedTypes.Contains(property.PropertyType) || excludedNames.Contains(property.Name))
                    continue;
                sb.Append(shift + property.Name);
                if (SpecialSerializationForNames.ContainsKey(property.Name))
                    sb.Append(" = " + SpecialSerializationForNames[property.Name](property.GetValue(obj)));
                else
                    sb.Append(property.GetValue(obj) is ICollection
                        ? " : " + PrintCollection(((ICollection) property.GetValue(obj))!, nestingLevel + 1)
                        : " = " + PrintToString(property.GetValue(obj), nestingLevel + 1));
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            if (nestingLevel > 10)
                throw new ArgumentException("Cycling reference");
            var shift = new string('\t', nestingLevel + 2);
            var collectionLength = collection.Count;
            var sb = new StringBuilder();
            var openBracket = collection is IDictionary ? '{' : '[';
            var closeBracket = collection is IDictionary ? '}' : ']';
            sb.Append(openBracket);
            if (collection is IDictionary dictionary)
                sb.Append(PrintDictionary(dictionary, nestingLevel + 2));
            else
            {
                sb.Append(Environment.NewLine);
                foreach (var element in collection)
                {
                    sb.Append(shift);
                    sb.Append(PrintElementInCollection(element, nestingLevel, shift));
                    if (collectionLength - 1 > 0)
                        sb.Append(",");
                    sb.Append(Environment.NewLine);
                    collectionLength--;
                }
            }

            sb.Append(new string('\t', nestingLevel + 1) + closeBracket);
            return sb.ToString();
        }

        private string PrintElementInCollection(object element, int nestinngLevel, string shift)
        {
            if (element is ICollection nestedCollection)
                return PrintCollection(nestedCollection, nestinngLevel + 1);
            var result = PrintToString(element, nestinngLevel + 2);
            if (!finalTypes.Contains(element.GetType()))
                return result + shift;
            return result;
        }

        private string PrintDictionary(IDictionary dict, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            var dictLength = dict.Count;
            foreach (DictionaryEntry e in dict)
            {
                sb.Append(new string('\t', nestingLevel));
                sb.Append(e.Key is ICollection key
                    ? PrintCollection(key, nestingLevel)
                    : PrintToString(e.Key, nestingLevel));
                sb.Append(" : ");
                sb.Append(e.Value is ICollection value
                    ? PrintCollection(value, nestingLevel)
                    : PrintToString(e.Value, nestingLevel));
                if (dictLength - 1 > 0)
                    sb.Append(',');
                sb.Append(Environment.NewLine);
                dictLength--;
            }

            return sb.ToString();
        }
    }
}