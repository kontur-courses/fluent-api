using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        private HashSet<Type> excludedTypes;
        private HashSet<PropertyInfo> excludedProperties;
        private HashSet<object> recursedProperties;
        
        private Dictionary<Type, Func<object, string>> alternativeSerializationForType;
        private Dictionary<PropertyInfo, Func<object, string>> alternativeSerializationForProperty;
        
        private Dictionary<Type, CultureInfo> cultureInfoForType;
        
        private Dictionary<PropertyInfo, int> stringPropertiesMaxLenght;

        public PrintingConfig()
        {
            excludedProperties = new HashSet<PropertyInfo>();
            excludedTypes = new HashSet<Type>();
            recursedProperties = new HashSet<object>();
            alternativeSerializationForType = new Dictionary<Type, Func<object, string>>();
            alternativeSerializationForProperty = new Dictionary<PropertyInfo, Func<object, string>>();
            cultureInfoForType = new Dictionary<Type, CultureInfo>();
            stringPropertiesMaxLenght = new Dictionary<PropertyInfo, int>();
        }

        public void AddCultureForType(Type type, CultureInfo info)
        {
            if (cultureInfoForType.ContainsKey(type))
            {
                cultureInfoForType[type] = info;
                return;
            }
            cultureInfoForType.Add(type, info);
        }
        public void AddAlternativeSerializationForType(Type type, Func<object, string> serializer)
        {
            if (alternativeSerializationForType.ContainsKey(type))
            {
                alternativeSerializationForType[type] = serializer;
                return;
            }
            alternativeSerializationForType.Add(type, serializer);
        }
        public void AddAlternativeSerializationForProperty(PropertyInfo property, Func<object, string> serializer)
        {
            if (alternativeSerializationForProperty.ContainsKey(property))
            {
                alternativeSerializationForProperty[property] = serializer;
                return;
            }
            alternativeSerializationForProperty.Add(property, serializer);
        }
        public void AddMaxLenghtForStringProperty(PropertyInfo info, int maxLenght)
        {
            if (stringPropertiesMaxLenght.ContainsKey(info))
            {
                stringPropertiesMaxLenght[info] = maxLenght;
                return;
            }
            stringPropertiesMaxLenght.Add(info, maxLenght);
        }
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyFromMemberSelector(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(GetPropertyFromMemberSelector(memberSelector));
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

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (finalTypes.Contains(type))
            {
                if (cultureInfoForType.ContainsKey(type))
                    return ParseWithCulture(obj, cultureInfoForType[type]) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            DeleteRecursion(obj);
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            
            if(obj is IDictionary dictionary)
                return SerialzieDictionary(dictionary, nestingLevel);
            if (obj is IEnumerable enumerable)
                return SerialzieEnumerable(enumerable, nestingLevel);
            foreach (var propertyInfo in type.GetProperties())
            {
                var header = identation + propertyInfo.Name + " = ";
                var propType = propertyInfo.PropertyType;
                var propertyObj = propertyInfo.GetValue(obj);
                
                if(excludedTypes.Contains(propType) || excludedProperties.Contains(propertyInfo))
                    continue;
                if (recursedProperties.Contains(propertyObj)) 
                {
                    sb.Append(header + "Recursion");
                    continue;
                }
                if (alternativeSerializationForProperty.ContainsKey(propertyInfo))
                {
                    sb.Append(header + alternativeSerializationForProperty[propertyInfo](propertyObj)+Environment.NewLine);
                    continue;
                }
                if (alternativeSerializationForType.ContainsKey(propType))
                {
                    sb.Append(header + alternativeSerializationForType[propType](propertyObj));
                    continue;
                }
                
                if (propType == typeof(string))
                {
                    var value = (string)propertyObj;
                    var trimmedString = value?.Substring(0,
                        Math.Min(
                            stringPropertiesMaxLenght.ContainsKey(propertyInfo)
                                ? stringPropertiesMaxLenght[propertyInfo]
                                : Int32.MaxValue
                            , value.Length));
                    sb.Append(header +
                              PrintToString(trimmedString,
                                  nestingLevel + 1));
                    continue;
                }
                sb.Append(header +
                          PrintToString(propertyObj,
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        #region HelperMethods

        private string SerialzieEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder("\n");
            var identation = new string('\t', nestingLevel + 1);
            foreach (var obj in enumerable)
            {
                sb.Append(identation +
                          PrintToString(obj,
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
        
        private string SerialzieDictionary(IDictionary dictionary, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder("Dictionary\n");
            
            var keys = dictionary.Keys.Cast<object>().ToArray();
            var values = dictionary.Values.Cast<object>().ToArray();
            if (keys.Length != values.Length)
                throw new Exception();
            
            for(int i =0; i<keys.Length; i++)
            {
                sb.Append(identation + 
                          "Key:" +
                          PrintToString(keys[i], nestingLevel + 1) +
                          identation +
                          "Value:" + 
                          PrintToString(values[i], nestingLevel + 1));
            }

            return sb.ToString();
        }
        
        private void DeleteRecursion(object parentObj) => DeleteRecursion(parentObj, parentObj);
        private void DeleteRecursion(object parentObj, object childObject)
        {
            if(childObject == null)
                return;
            if(recursedProperties.Contains(childObject))
                return;
            var type = childObject.GetType();
            if (type.GetProperties().Length == 0 || finalTypes.Contains(type))
                return;
            if (childObject is IDictionary dict)
                DeleteRecursionInDictionary(parentObj, dict);
            else if(childObject is IEnumerable enumerable)
                DeleteRecursionInEnumerable(parentObj, enumerable);
            else foreach (var propertyInfo in type.GetProperties())
            {
                var childPropertyObj = propertyInfo.GetValue(childObject);
                if(childPropertyObj == null)
                    continue;
                if (childPropertyObj == parentObj)
                {
                    recursedProperties.Add(childPropertyObj);
                    continue;
                }
                DeleteRecursion(parentObj, childPropertyObj);
            }
        }

        private void DeleteRecursionInEnumerable(object parentObj, IEnumerable enumerable)
        {
            foreach (var obj in enumerable)
                DeleteRecursion(parentObj, obj);
        }

        private void DeleteRecursionInDictionary(object parentObj, IDictionary dict)
        {
            foreach (var key in dict.Keys)
                DeleteRecursion(parentObj, key);
            foreach (var value in dict.Values)
                DeleteRecursion(parentObj, value);
        }

        private string ParseWithCulture(object o, CultureInfo cultureInfo)
        {
            switch (o)
            {
                case double d:
                    return d.ToString("G", cultureInfo);
                case string s:
                    return string.Format(cultureInfo, s);
                default:
                    return o.ToString();
            }
        }

        private PropertyInfo GetPropertyFromMemberSelector<TOwner, TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Expression is not property");
            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Expression is not property");
            return propInfo;
        }

        #endregion
    }
}