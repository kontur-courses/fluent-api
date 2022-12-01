using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludeTypes = new List<Type>();
        
        private readonly List<PropertyInfo> excludeProperties = new List<PropertyInfo>();

        private readonly Dictionary<PropertyInfo, object> propertiesConfig = new Dictionary<PropertyInfo, object>();

        private readonly Dictionary<Type, object> typesConfig = new Dictionary<Type, object>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            typesConfig[typeof(TPropType)] = propertyConfig;
            return propertyConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            var member = (MemberExpression)memberSelector.Body;
            var property = (PropertyInfo)member.Member;
            propertiesConfig[property] = propertyConfig;
            return propertyConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = (MemberExpression)memberSelector.Body;
            var property = (PropertyInfo)member.Member;
            excludeProperties.Add(property);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludeTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj) 
        {
            var str = new StringBuilder();
            if (obj is IDictionary dictionary)
            {
                str.Append("[");
                foreach (var item in dictionary)
                {
                    var itemDictionary = (DictionaryEntry)item;
                    str.Append("[");
                    str.Append(PrintToString(itemDictionary.Key, 1));
                    str.Append("]");
                    str.Append(" = ");
                    str.Append(PrintToString(itemDictionary.Value, 1));
                    str.Append(", ");
                }
                if (str.Length > 2)
                    str.Remove(str.Length - 2, 2);
                str.Append("]");
            }

            else if (obj is IEnumerable enumerable)
            {
                str.Append("[");
                foreach (var item in enumerable)
                {
                    str.Append(PrintToString(item, 0));
                    str.Append(", ");
                }
                if (str.Length > 2)
                    str.Remove(str.Length - 2, 2);
                str.Append("]");
            }

            else 
                str.Append(PrintToString(obj, 0));
            
            return str.ToString();
        }

        private string PrintToString(object obj, int nestingLevel, PropertyInfo prInfo = null)
        {
            if (nestingLevel > 50)
                return " ";
            
            if (obj == null)
                return "null" + ' ';

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                var objValue = obj.ToString();
                if (obj is string stringObj)
                    objValue = ApplyRulesForString(stringObj, prInfo);
                else if (obj is int intObj)
                    objValue = ApplyRulesForNotString(intObj, prInfo);
                else if (obj is double doubleObj)
                    objValue = ApplyRulesForNotString(doubleObj, prInfo);
                else if (obj is DateTime dateTimeObj)
                    objValue = ApplyRulesForNotString(dateTimeObj, prInfo);
                else if (obj is float floatObj)
                    objValue = ApplyRulesForNotString(floatObj, prInfo);
                else if (obj is TimeSpan timeSpanObj)
                    objValue = ApplyRulesForNotString(timeSpanObj, prInfo);
                return objValue;
            }
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludeTypes.Contains(propertyInfo.PropertyType) || excludeProperties.Contains(propertyInfo))
                    continue;

                sb.Append(' ' + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, propertyInfo));
            }
            return sb.ToString();
        }

        private string ApplyRulesForNotString<TPropType>(TPropType obj, PropertyInfo prInfo)
        {
            var objValue = obj.ToString();
            if (typesConfig.ContainsKey(typeof(TPropType)) 
                && typesConfig[typeof(TPropType)] is PropertyPrintingConfig<TOwner, TPropType> config)
            {
                if (!(config.PropertyRule is null))
                    objValue = config.PropertyRule(obj);
            }

            if (!(prInfo is null) && propertiesConfig.ContainsKey(prInfo) 
                                  && propertiesConfig[prInfo] is PropertyPrintingConfig<TOwner, TPropType> propConfig)
                if (!(propConfig.PropertyRule(obj) is null))
                    objValue = propConfig.PropertyRule(obj);
            
            return objValue;
        }

        private string ApplyRulesForString(string obj, PropertyInfo prInfo) 
        {
            var objValue = obj;
            if (typesConfig.ContainsKey(typeof(string)) && typesConfig[typeof(string)] is PropertyPrintingConfig<TOwner, string> config)
            {
                if (!(config.PropertyRule is null))
                {
                    objValue = config.PropertyRule(obj);
                    obj = objValue;
                }
            }

            if (!(prInfo is null) && propertiesConfig.ContainsKey(prInfo) 
                                  && propertiesConfig[prInfo] is PropertyPrintingConfig<TOwner, string> propConfig)
                if (!(propConfig.PropertyRule is null))
                    objValue = propConfig.PropertyRule(obj);

            return objValue;
        }
    }
}