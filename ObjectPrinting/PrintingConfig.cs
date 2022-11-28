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

        //public Dictionary<Type, PropertyPrintingConfig<TOwner, object>> TypesFunc;
        public PropertyPrintingConfig<TOwner, string> StringConfig{ get; set; }
        public PropertyPrintingConfig<TOwner, int> IntConfig { get; set; }
        public PropertyPrintingConfig<TOwner, double> DoubleConfig { get; set; }
        public PropertyPrintingConfig<TOwner, DateTime> DateTimeConfig { get; set; }
        public PropertyPrintingConfig<TOwner, TimeSpan> TimeSpanConfig { get; set; }
        public PropertyPrintingConfig<TOwner, Guid> GuidConfig { get; set; }

        //public PrintingConfig()
        //{
        //    //StringConfig = new PropertyPrintingConfig<TOwner, string>(this);
        //    //IntConfig = new PropertyPrintingConfig<TOwner, int>(this);
        //    //DoubleConfig = new PropertyPrintingConfig<TOwner, double>(this);
        //    //DateTimeConfig = new PropertyPrintingConfig<TOwner, DateTime>(this);
        //    //TimeSpanConfig = new PropertyPrintingConfig<TOwner, TimeSpan>(this);
        //    //GuidConfig = new PropertyPrintingConfig<TOwner, Guid>(this);
        //}

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            if (propertyConfig is PropertyPrintingConfig<TOwner, string> stringConfig)
            {
                if(StringConfig is null)
                    StringConfig = stringConfig;
                else
                {
                    var propertyRule = StringConfig.PropertyRule;
                    StringConfig = stringConfig;
                    StringConfig.PropertyRule = propertyRule;
                }
            }
            //дописать для остальных типов данных
            else if (propertyConfig is PropertyPrintingConfig<TOwner, int> intConfig)
                IntConfig = intConfig;
            else if (propertyConfig is PropertyPrintingConfig<TOwner, double> doubleConfig)
                DoubleConfig = doubleConfig;
            else if (propertyConfig is PropertyPrintingConfig<TOwner, DateTime> dateTimeConfig)
                DateTimeConfig = dateTimeConfig;
            else if (propertyConfig is PropertyPrintingConfig<TOwner, TimeSpan> timeSpanConfig)
                TimeSpanConfig = timeSpanConfig;
            else if (propertyConfig is PropertyPrintingConfig<TOwner, Guid> guidConfig)
                GuidConfig = guidConfig;
            return propertyConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
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


        private string PrintToString(object obj, int nestingLevel)
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
                if (!(StringConfig is null) && obj is string stringObj)
                    objValue = StringConfig.PropertyRule(stringObj);
                else if (!(DoubleConfig is null) && obj is double doubleObj)
                {
                    //if (!(DoubleConfig.PropertyRule is null))
                    //    objValue = DoubleConfig.PropertyRule(doubleObj);
                    if (!(DoubleConfig.CultureInfo is null))
                        objValue = doubleObj.ToString(DoubleConfig.CultureInfo);
                }
                //дописать для остальных типов данных
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
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}