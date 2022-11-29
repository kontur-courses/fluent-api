using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludeTypes = new List<Type>();
        
        private readonly List<PropertyInfo> excludeProperties = new List<PropertyInfo>();

        public Dictionary<PropertyInfo, List<object>> Properties { get; set; }
        
        public PrintingConfig()
        {
            Properties = new Dictionary<PropertyInfo, List<object>>();
            foreach (var propertyInfo in typeof(TOwner).GetProperties())
                Properties[propertyInfo] = new List<object>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            foreach (var property in Properties)
            {
                if (property.Key.PropertyType == typeof(TPropType))
                    Properties[property.Key].Add(propertyConfig);
            }

            return propertyConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            var member = (MemberExpression)memberSelector.Body;
            var property = (PropertyInfo)member.Member;
            Properties[property].Add(propertyConfig);
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
                if (obj is string stringObj && !(prInfo is null) && Properties.ContainsKey(prInfo))
                {
                    foreach (var prConfig in Properties[prInfo])
                    {
                        if (prConfig is PropertyPrintingConfig<TOwner, string> stringConfig)
                        {
                            objValue = stringConfig.PropertyRule(stringObj);
                            stringObj = objValue;
                        }
                    }
                }

                if (obj is int intObj && !(prInfo is null) && Properties.ContainsKey(prInfo))
                {
                    foreach (var prConfig in Properties[prInfo])
                    {
                        if (prConfig is PropertyPrintingConfig<TOwner, int> intConfig)
                        {
                            if(!(intConfig.PropertyRule is null))
                                objValue = intConfig.PropertyRule(intObj);
                            else if (!(intConfig.CultureInfo is null))
                                objValue = intObj.ToString(intConfig.CultureInfo);
                        }
                    }
                }

                if (obj is double doubleObj && !(prInfo is null) && Properties.ContainsKey(prInfo))
                {
                    foreach (var prConfig in Properties[prInfo])
                    {
                        if (prConfig is PropertyPrintingConfig<TOwner, double> doubleConfig)
                        {
                            if (!(doubleConfig.PropertyRule is null))
                                objValue = doubleConfig.PropertyRule(doubleObj);
                            else if (!(doubleConfig.CultureInfo is null))
                                objValue = doubleObj.ToString(doubleConfig.CultureInfo);
                        }
                    }
                }
                return objValue + Environment.NewLine;
                    //return obj + Environment.NewLine;
                
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
    }
}