using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Equivalency;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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
            return PrintToString(obj, 0);
        }


        private string PrintToString(object obj, int nestingLevel, PropertyInfo prInfo = null)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

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
                    objValue = ApplyRulesForNumbers(intObj, prInfo);
                else if (obj is double doubleObj)
                    objValue = ApplyRulesForNumbers(doubleObj, prInfo);
                return objValue + Environment.NewLine;
            }
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludeTypes.Contains(propertyInfo.PropertyType) || excludeProperties.Contains(propertyInfo))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, propertyInfo));
            }
            return sb.ToString();
        }

        private string ApplyRulesForNumbers<TPropType>(TPropType obj, PropertyInfo prInfo) where TPropType : IConvertible
        {
            var objValue = obj.ToString();
            if (typesConfig.ContainsKey(typeof(TPropType)) && typesConfig[typeof(TPropType)] is PropertyPrintingConfig<TOwner, TPropType> config)
            {
                if (!(config.PropertyRule is null))
                    objValue = config.PropertyRule(obj);
                else if (!(config.CultureInfo is null))
                    objValue = obj.ToString(config.CultureInfo);
            }

            if (!(prInfo is null) && propertiesConfig.ContainsKey(prInfo) && propertiesConfig[prInfo] is PropertyPrintingConfig<TOwner, TPropType> propConfig)
                if (!(propConfig.PropertyRule(obj) is null))
                    objValue = propConfig.PropertyRule(obj);
            
            return objValue;
        }

        private string ApplyRulesForString(string obj, PropertyInfo prInfo) 
        {
            var objValue = obj.ToString();
            if (typesConfig.ContainsKey(typeof(string)) && typesConfig[typeof(string)] is PropertyPrintingConfig<TOwner, string> config)
            {
                if (!(config.PropertyRule is null))
                {
                    objValue = config.PropertyRule(obj);
                    obj = objValue;
                }
            }

            if (!(prInfo is null) && propertiesConfig.ContainsKey(prInfo) && propertiesConfig[prInfo] is PropertyPrintingConfig<TOwner, string> propConfig)
                if (!(propConfig.PropertyRule is null))
                    objValue = propConfig.PropertyRule(obj);

            return objValue;
        }
    }
}