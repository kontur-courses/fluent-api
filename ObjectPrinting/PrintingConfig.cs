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
        private List<Type> excludedTypes = new List<Type>();
        private List<PropertyInfo> excludedProperties = new List<PropertyInfo>();
        private Dictionary<Type, Delegate> serializedByType = new Dictionary<Type, Delegate>();
        private Dictionary<PropertyInfo, Delegate> serializedByPropertyInfo = new Dictionary<PropertyInfo, Delegate>();
        private HashSet<object> printedObjects = new HashSet<object>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)member);
        }
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    memberSelector.ToString()));
            }

            if (!(memberExpression.Member is PropertyInfo propInfo))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    memberSelector.ToString()));
            }
            excludedProperties.Add(propInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        private bool IsFinalType(Type type)
        {
            return type.IsValueType ||
                   (type.IsPrimitive || type == typeof(string));
        }
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            if (printedObjects.Contains(obj))
                return sb.AppendLine("(Cycle) " + type.FullName).ToString();
            sb.AppendLine(type.Name);
            printedObjects.Add(obj);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (IsExcluded(propertyInfo)) continue;
                if (serializedByPropertyInfo.ContainsKey(propertyInfo))
                {
                    var serializedValue = serializedByPropertyInfo[propertyInfo].DynamicInvoke(propertyInfo.GetValue(obj));;
                    sb.AppendLine(identation + propertyInfo.Name + " = " + serializedValue);
                    continue;
                }

                if (serializedByType.ContainsKey(propertyInfo.PropertyType))
                {
                    var serializedValue = serializedByType[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj));;
                    sb.AppendLine(identation + propertyInfo.Name + " = " + serializedValue);
                    continue;
                }
                
                if (propertyInfo.PropertyType.IsArray || (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    sb.AppendLine(identation + propertyInfo.Name + " = ");
                    var list = (IList)propertyInfo.GetValue(obj);
                    sb.Append(SerializateEnumerable(list, nestingLevel + 1));
                    continue;
                }
                
                if (propertyInfo.PropertyType.IsGenericType && (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) || propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                {
                        sb.AppendLine(identation + propertyInfo.Name + " = ");
                        var dictionary = (IDictionary)propertyInfo.GetValue(obj);
                        sb.Append(SerializateDictionary(dictionary, nestingLevel + 1));
                        continue;
                }
                
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string SerializateEnumerable(IEnumerable dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            if(dictionary == null) return "null" + Environment.NewLine;
            foreach (var value in dictionary)
            {
                sb.Append(PrintToString(value, nestingLevel));
            }
            return sb.ToString();
        }
        private string SerializateDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            if(dictionary == null) return "null" + Environment.NewLine;
            foreach (var keyVal in dictionary)
            {
                var identation = new string('\t', nestingLevel + 1);
                sb.Append(identation);
                var key = ((DictionaryEntry)keyVal).Key;
                var value = ((DictionaryEntry)keyVal).Value;
                sb.Append(PrintToString(key, nestingLevel) + " : " + PrintToString(value, nestingLevel));
            }

            return sb.ToString();
        }
        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return excludedProperties.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);
        }
        
        public void AddSerializateProperty<TPropType>(Func<TPropType, string> print, PropertyInfo propertyInfo)
        {
            serializedByPropertyInfo.Add(propertyInfo, print);
        }
        
        public void AddSerializateByType<TPropType>(Type type, Func<TPropType, string> print)
        {
            serializedByType.Add(type, print);
        }
    }
}