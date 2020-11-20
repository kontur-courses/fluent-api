using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : SerializationConfig<TOwner>
    {
        private readonly Type[] finalTypes =
            {typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan)};
        private readonly HashSet<object> parents = new HashSet<object>();

        public PrintingConfig()
        {
            ParentConfig = this;
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0) ?? "Cyclic reference found";
        
        private string PrintToString(object obj, int nestingLevel)
        {
            if (parents.Contains(obj))
                return null;

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var elements = obj switch
            {
                IDictionary _ => SerializeDictionaryElements(obj, nestingLevel),
                IEnumerable _ => SerializeEnumerableElements(obj, nestingLevel),
                _ => SerializeProperties(obj, nestingLevel)
            };
            if (elements == null) return null;
            return type.Name + Environment.NewLine + elements;
        }

        private string SerializeProperties(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            parents.Add(obj);
            foreach (var propertyInfo in type.GetProperties().Where(t => !IsExcluded(t)))
            {
                dynamic value = propertyInfo.GetValue(obj);
                var serializedProperty = PropertyToString(propertyInfo, value, nestingLevel);
                if (serializedProperty == null) return null;
                sb.Append(indentation + propertyInfo.Name + " = " + serializedProperty);
            }
            parents.Remove(obj);
            return sb.ToString();
        }
        
        private string PropertyToString(PropertyInfo propertyInfo, dynamic value, int nestingLevel) =>
            GetAlternativeSerialization(propertyInfo, value) ?? PrintToString(value, nestingLevel + 1);
        
        private string SerializeEnumerableElements(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var arrayIndentation = new string('\t', nestingLevel);
            var indentation = new string('\t', nestingLevel + 1);
            
            sb.AppendLine(arrayIndentation + "[");
            foreach (var item in (IEnumerable)obj)
            {
                var serializedItem = PrintToString(item, nestingLevel + 1);
                if (serializedItem == null) return null;
                sb.Append(indentation + serializedItem);
            }
            sb.AppendLine(arrayIndentation + "]");
            
            return sb.ToString();
        }
        
        private string SerializeDictionaryElements(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var arrayIndentation = new string('\t', nestingLevel);
            var indentation = new string('\t', nestingLevel + 1);
            var dict = (IDictionary) obj;
            var keys = new object[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            var values = new object[dict.Count];
            dict.Values.CopyTo(values, 0);
            
            sb.AppendLine(arrayIndentation + "[");
            foreach (var item in keys.Zip(values).ToDictionary(pair => pair.First, pair => pair.Second))
            {
                var serializedKey = PrintToString(item.Key, nestingLevel + 1);
                if (serializedKey == null) return null;
                var serializedValue = PrintToString(item.Value, nestingLevel + 1);
                if (serializedValue == null) return null;
                sb.Append(indentation + "[" + RemoveNewLineAtEnd(serializedKey) + "] = " + serializedValue);
            }
            sb.AppendLine(arrayIndentation + "]");
            
            return sb.ToString();
        }

        private static string RemoveNewLineAtEnd(string s) => s.TrimEnd(Environment.NewLine.ToCharArray());
    }
}