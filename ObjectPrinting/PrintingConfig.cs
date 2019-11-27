using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> 
    {
        private HashSet<Type> excludedTypes = new HashSet<Type>();       
        private HashSet<string> excludedProperties = new HashSet<string>();
        private HashSet<object> visited = new HashSet<object>();

        private readonly Dictionary<Type, Func<object, string>> typesSerializationDict = new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<string, Func<object, string>> propertiesSerializationDict = new Dictionary<string, Func<object, string>>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool)
        };

        private readonly int currentNestingLevel = 5;
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = GetPropertyName(memberSelector);
            if (propertyName != null)
            {
                excludedProperties.Add(propertyName);
            }
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

        public void SetTypeSerialization<TPropType>(Func<TPropType, string> print)
        {
            typesSerializationDict.Add(typeof(TPropType), x => print((TPropType)x));
        }

        
        public void SetMemberSerialization<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector, Func<TPropType, string> print)
        {
            if (memberSelector.Body is MemberExpression memberExpr)
            {                
                var propertyName = memberExpr.Member.Name;
                Func<object, string> func = x => print((TPropType)x);
                propertiesSerializationDict.Add(propertyName, func);
            }
        }
                
        private string PrintToString(object obj, int nestingLevel)
        {
            if (currentNestingLevel == nestingLevel)
                return "Nesting level exceeded" + Environment.NewLine;
            if (obj == null)
                return "null" + Environment.NewLine;
            var objType = obj.GetType();
            if (visited.Contains(obj))
                return "Circular reference" + Environment.NewLine;
            if (objType.IsClass)
                visited.Add(obj);
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable enumerable)            
                AddElementsFromCollection(enumerable, sb,identation, nestingLevel);                        
            else
                AddProperties(obj, nestingLevel, sb);
            return sb.ToString();            
        }

        private void AddElementsFromCollection(IEnumerable enumerable, StringBuilder sb, string identation, int nestingLevel)
        {            
            if (enumerable is IDictionary dictionary)
                foreach (DictionaryEntry pair in dictionary)
                    sb.Append(identation + pair.Key + " : " + PrintToString(pair.Value, nestingLevel + 1));
            else
            {
                var index = 0;
                foreach (var item in enumerable)
                    sb.Append(identation + index++.ToString() + " : " + PrintToString(item, nestingLevel + 1));
            }
        }

        private void AddProperties(object obj, int nestingLevel, StringBuilder sb)
        {
            var objType = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);

            foreach (var property in objType.GetProperties())
            {
                if (CheckExcludingForType(property) || CheckExcludingForProperty(property))
                    continue;                
                
                if (propertiesSerializationDict.ContainsKey(property.Name))
                {
                    sb.Append(identation + property.Name + " = " +
                              propertiesSerializationDict[property.Name].Invoke(property.GetValue(obj))
                              + Environment.NewLine);
                    continue;
                }
                else if (typesSerializationDict.ContainsKey(property.PropertyType))
                {
                    sb.Append(identation + property.Name + " = " +
                              typesSerializationDict[property.PropertyType]
                                  .Invoke(property.GetValue(obj))
                              + Environment.NewLine);
                    continue;
                }
                
                var propertyString = PrintToString(property.GetValue(obj), nestingLevel + 1);
                sb.Append(identation + property.Name + " = " + propertyString);
            }
        }

        private bool CheckExcludingForType(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType);
        }

        private bool CheckExcludingForProperty(PropertyInfo propertyInfo)
        {
            return excludedProperties.Contains(propertyInfo.Name);
        }

        private static string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpr)
            {
                return memberExpr.Member.Name;
            }
            return null;
        }        
    }
}