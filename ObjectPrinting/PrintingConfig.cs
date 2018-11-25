using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludingProperties = new HashSet<PropertyInfo>();

        private readonly Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>> printingPropertiesConfigs 
            = new Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>>();
        private readonly Dictionary<Type, IPropertyPrintingConfig<TOwner>> printingTypesConfigs
            = new Dictionary<Type, IPropertyPrintingConfig<TOwner>>();

        private static Expression<Func<object, string>> defaultPrintingMethod = obj => obj.ToString();

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
      
        private readonly Type[] numberTypes = new[]
        {
            typeof(long), typeof(int), typeof(double), typeof(float)
        };

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression) propSelector.Body).Member;
            excludingProperties.Add(propertyInfo);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            PropertyPrintingConfig<TOwner, TPropType> propConf;
            var propType = typeof(TPropType);

            if (propType == typeof(string))
            {
                propConf = new StringPropertyConfig<TOwner, TPropType>(this);
            }
            else if (numberTypes.Contains(propType))
            {
                propConf = new NumberPropertyConfig<TOwner, TPropType>(this);
            }
            else
            {
                propConf = new PropertyPrintingConfig<TOwner,TPropType>(this);
            }

            if (!printingTypesConfigs.ContainsKey(propType))
            {
                printingTypesConfigs.Add(typeof(TPropType), propConf);
            }
            else
            {
                propConf = (PropertyPrintingConfig<TOwner, TPropType>)printingTypesConfigs[propType];
            }

            return propConf;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
        {
            PropertyPrintingConfig<TOwner, TPropType> propConf;
            var propertyInfo = (PropertyInfo)((MemberExpression) propSelector.Body).Member;
            var propType = typeof(TPropType);

            if (propType == typeof(string))
            {
                propConf = new StringPropertyConfig<TOwner, TPropType>(this, propertyInfo);
            }
            else if (numberTypes.Contains(propType))
            {
                propConf = new NumberPropertyConfig<TOwner, TPropType>(this, propertyInfo);
            }
            else
            {
                propConf = new PropertyPrintingConfig<TOwner,TPropType>(this, propertyInfo);
            }

            if (!printingPropertiesConfigs.ContainsKey(propertyInfo))
            {
                printingPropertiesConfigs.Add(propertyInfo, propConf);
            }
            else
            {
                propConf = (PropertyPrintingConfig<TOwner, TPropType>)printingPropertiesConfigs[propertyInfo];
            }

            return propConf;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return PrintLine("null");

            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties().Except(excludingProperties)
                .Where(prop => !excludingTypes.Contains(prop.PropertyType)))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          (
                              finalTypes.Contains(propertyInfo.PropertyType)
                                  ? PrintLine(PrintProperty(obj, propertyInfo))
                                  : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)
                          ));
            }
            return sb.ToString();
        }

        private string PrintProperty(object obj, PropertyInfo propertyInfo)
        {
            LambdaExpression printingMethod = null;
            if (propertyInfo.GetValue(obj) == null)
                return "null";

            var propertyType = propertyInfo.PropertyType;

            if (printingTypesConfigs.ContainsKey(propertyType))
            {
                printingMethod = printingTypesConfigs[propertyType].PrintingMethod;
            }

            if (printingPropertiesConfigs.ContainsKey(propertyInfo))
            {
                printingMethod = printingPropertiesConfigs[propertyInfo].PrintingMethod ?? printingMethod;
            }

            printingMethod = printingMethod ?? defaultPrintingMethod;
            var printingResult = printingMethod.Compile().DynamicInvoke(propertyInfo.GetValue(obj));

            if (propertyType == typeof(string))
            {
                var printingResultString = printingResult.ToString();
                var trimLength = printingTypesConfigs.ContainsKey(typeof(string))
                    ? ((StringPropertyConfig<TOwner, string>)printingTypesConfigs[typeof(string)]).TrimLength
                    : null;
                trimLength = printingPropertiesConfigs.ContainsKey(propertyInfo)
                    ? ((StringPropertyConfig<TOwner, string>) printingPropertiesConfigs[propertyInfo]).TrimLength ?? trimLength
                    : trimLength;
                return printingResultString
                    .Substring(0, Math.Min(printingResultString.Length, trimLength ?? int.MaxValue));
            }

            if (numberTypes.Contains(propertyType))
            {
                var cultureInfo = printingTypesConfigs.ContainsKey(propertyType) 
                    ? ((INumberPrintingConfig)printingTypesConfigs[propertyType]).CultureInfo
                    : null;
                cultureInfo = printingPropertiesConfigs.ContainsKey(propertyInfo)
                    ? ((INumberPrintingConfig) printingPropertiesConfigs[propertyInfo]).CultureInfo ?? cultureInfo
                    : cultureInfo;
                return cultureInfo != null ? printingResult.ToString().ToString(cultureInfo) : printingResult.ToString();
            }

            return printingResult.ToString();
        }

        //private LambdaExpression AllowCultureInfoToPrintingMethod(LambdaExpression printingMethod)
        //{
        //    printingMethod.Body.
        //}

        private string PrintLine(string value)
        {
            return value + Environment.NewLine;
        }
    }
}