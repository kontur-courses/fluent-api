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
        private HashSet<object> visited;

        private readonly Dictionary<Type, Func<object, string>> typesPrintingFunctions = new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<string, Func<object, string>> propertiesPrintingFunctions = new Dictionary<string, Func<object, string>>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(bool)
        };

        private int currentNestingLevel = 5;
        
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
            visited = new HashSet<object>();
            return PrintObjectToString(obj, 0);
        }        

        
        public string PrintToString(TOwner obj, int nestingLevel)
        {
            visited = new HashSet<object>();
            currentNestingLevel = nestingLevel;
            return PrintObjectToString(obj, 0);
        }        


        public void SetTypeSerialization<TPropType>(Func<TPropType, string> print)
        {
            typesPrintingFunctions.Add(typeof(TPropType), x => print((TPropType)x));
        }

        
        public void SetMemberSerialization<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector, Func<TPropType, string> print)
        {
            if (memberSelector.Body is MemberExpression memberExpr)
            {                
                var propertyName = memberExpr.Member.Name;
                Func<object, string> func = x => print((TPropType)x);
                propertiesPrintingFunctions.Add(propertyName, func);
            }
        }
                
        private string PrintObjectToString(object obj, int nestingLevel)
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

            var indentation = new string('\t', nestingLevel + 1);
            var objectStringBuilder = new StringBuilder();
            var type = obj.GetType();
            objectStringBuilder.AppendLine(type.Name);
            if (obj is IEnumerable enumerable)
                objectStringBuilder.Append(AddElementsFromCollection(enumerable, indentation, nestingLevel));                        
            else
                objectStringBuilder.Append(AddProperties(obj, nestingLevel));
            return objectStringBuilder.ToString();            
        }

        private string AddElementsFromCollection(IEnumerable enumerable, string indentation, int nestingLevel)
        {
            var collectionStringBuilder = new StringBuilder();
            if (enumerable is IDictionary dictionary)
                foreach (DictionaryEntry pair in dictionary)
                    collectionStringBuilder.Append($"{indentation} {pair.Key} : {PrintObjectToString(pair.Value, nestingLevel + 1)}");
            else
            {
                var index = 0;
                foreach (var item in enumerable)
                    collectionStringBuilder.Append($"{indentation} {index++.ToString()} : {PrintObjectToString(item, nestingLevel + 1)}");
            }
            return collectionStringBuilder.ToString();
        }

        private string AddProperties(object obj, int nestingLevel)
        {
            var propertiesStringBuilder = new StringBuilder();
            var objType = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);

            foreach (var property in objType.GetProperties())
            {
                if (CheckExcludingForType(property) || CheckExcludingForProperty(property))
                    continue;                
                
                if (propertiesPrintingFunctions.ContainsKey(property.Name))
                {
                    propertiesStringBuilder.Append($"{indentation}{property.Name} = { propertiesPrintingFunctions[property.Name].Invoke(property.GetValue(obj))}{Environment.NewLine}");
                    continue;
                }
                if (typesPrintingFunctions.ContainsKey(property.PropertyType))
                {
                    propertiesStringBuilder.Append($"{indentation}{property.Name} = {typesPrintingFunctions[property.PropertyType].Invoke(property.GetValue(obj)) }{Environment.NewLine}");                    
                    continue;
                }
                
                var propertyString = PrintObjectToString(property.GetValue(obj), nestingLevel + 1);
                propertiesStringBuilder.Append($"{indentation} {property.Name} = {propertyString}");                
            }
            return propertiesStringBuilder.ToString();
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