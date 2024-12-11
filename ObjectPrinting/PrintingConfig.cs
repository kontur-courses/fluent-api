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
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<PropertyInfo> excludedFields = new();
        private readonly HashSet<object> proccesedObjects = new();
        private readonly Dictionary<Type, Func<object, string>> alternativeSerializeTypes = new();
        private readonly Dictionary<PropertyInfo, Func<object, string>> alternativeSerializeProperties = new();
        private readonly Dictionary<Type, CultureInfo> alternativeCulture = new();
        private readonly Dictionary<PropertyInfo, int> lengthOfProperties = new();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            var identation = new string('\t', nestingLevel + 1);

            var specialSerialization = CheckSerializationConditions(obj, identation);
            if (specialSerialization != null)
                return specialSerialization;

            if (obj is IDictionary dictionary)
                return PrintDictionary(dictionary, nestingLevel);
            if (obj is IEnumerable collection)
                return PrintCollection(collection, nestingLevel);

            proccesedObjects.Add(obj);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedFields.Contains(propertyInfo))
                    continue;
                if (lengthOfProperties.TryGetValue(propertyInfo, out var length))
                {
                    var substring = propertyInfo.GetValue(obj).ToString().Substring(0, length);
                    sb.Append(identation + propertyInfo.Name + " = " +
                              substring + Environment.NewLine);
                    continue;
                }

                if (alternativeSerializeProperties.TryGetValue(propertyInfo, out var propertyFunc))

                    sb.Append(identation + propertyInfo.Name + " = " +
                              propertyFunc(propertyInfo.GetValue(obj)) + Environment.NewLine);
                else
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }

            return sb.ToString();
        }


        private string CheckSerializationConditions(object obj, string identation)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (excludedTypes.Contains(obj.GetType()))
                return "";
            if (proccesedObjects.Contains(obj))
                return "Cyclic reference" + Environment.NewLine;
            if (alternativeSerializeTypes.TryGetValue(obj.GetType(), out var serializeFunc))
                return serializeFunc(obj) + Environment.NewLine;

            if (obj is IFormattable formObj && alternativeCulture.TryGetValue(obj.GetType(), out var newCulture))
                return formObj.ToString("N", newCulture) + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            return null;
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);

            sb.Append(identation + collection.GetType().Name + "{" + Environment.NewLine);
            int index = 0;
            foreach (var item in collection)
            {
                sb.Append(identation + "\t[" + index + "] = ");
                sb.Append(GetStringWithNestingLevel(item, nestingLevel + 2).Trim());
                sb.Append(Environment.NewLine);
                index++;
            }

            sb.Append(identation + "}");
            return sb.ToString();
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation + dictionary.GetType().Name + "{" + Environment.NewLine);
            foreach (DictionaryEntry element in dictionary)
            {
                var key = GetStringWithNestingLevel(element.Key, nestingLevel + 1);
                var value = GetStringWithNestingLevel(element.Value, nestingLevel + 2).Trim();
                sb.Append(key + " : " + value + Environment.NewLine);
            }

            sb.Append(identation + "}");
            return sb.ToString();
        }

        private string GetStringWithNestingLevel(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            if (finalTypes.Contains(obj.GetType()))
                return identation + obj;
            var result = PrintToString(obj, nestingLevel);
            return Environment.NewLine + identation + result;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> func)
        {
            var propertyInfo = GetPropertyInfo(func);
            excludedFields.Add(propertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> SerializeTypeWithSpecial<T>(Func<object, string> serializeFunc)
        {
            alternativeSerializeTypes[typeof(T)] = o => serializeFunc((T) o);
            return this;
        }

        public PrintingConfig<TOwner> SelectCulture<T>(CultureInfo cultureInfo)
        {
            alternativeCulture[typeof(T)] = cultureInfo;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> SelectField<TPropType>(
            Expression<Func<TOwner, TPropType>> func)
        {
            var propertyInfo = GetPropertyInfo(func);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        private PropertyInfo GetPropertyInfo<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            var propertyName = ((MemberExpression) propertyExpression.Body).Member.Name;
            var propertyInfo = typeof(TOwner).GetProperty(propertyName);
            if (propertyInfo == null)
                throw new Exception($"Property {propertyName} could not be found.");
            return propertyInfo;
        }

        public void AddSerializedProperty<T>(PropertyInfo propertyInfo, Func<T, string> serializeFunc)
        {
            alternativeSerializeProperties[propertyInfo] = o => serializeFunc((T) o);
        }

        public void AddLengthOfProperty(PropertyInfo propertyInfo, int length)
        {
            lengthOfProperties[propertyInfo] = length;
        }
    }
}