using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {

    }
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly HashSet<string> excludedPropertyNames = new HashSet<string>();

        private readonly Dictionary<Type, TypeConfig<TOwner>> typeConfingsDict = new Dictionary<Type, TypeConfig<TOwner>>();

        private readonly Dictionary<string, IPropertyConfig> propertyConfigsDict = new Dictionary<string, IPropertyConfig>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objType = obj.GetType();
            if (typeConfingsDict.TryGetValue(objType, out var typeConfig) && typeConfig.Func != null)
                return typeConfig.Func.Invoke(obj) + Environment.NewLine;

            if (finalTypes.Contains(objType))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objType.Name);
            foreach (var propertyInfo in objType.GetProperties())
            {
                if (!excludedTypes.Contains(propertyInfo.PropertyType))
                {
                    if (propertyConfigsDict.TryGetValue(propertyInfo.Name, out var propertyConfig) && propertyConfig.Func != null)
                    {
                        sb.Append(identation + propertyInfo.Name + " = " +
                                propertyConfig.Func.Invoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                    }
                    else if (!excludedPropertyNames.Contains(propertyInfo.Name))
                    {
                        sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
                    }
                }
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public TypeConfig<TOwner> Printing<T>()
        {
            var config = new TypeConfig<TOwner>(this, typeof(T));
            typeConfingsDict.Add(config.Type, config);
            return config;
        }

        public PropertyConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> expression)
        {
            var config = new PropertyConfig<TOwner, T>(this, expression);
            propertyConfigsDict.Add(config.PropertyName, config);
            return config;
        }

        public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
        {
            var strExpressionSplitted = expression.Body.ToString().Split('.');
            var toExclude = strExpressionSplitted[^1];
            excludedPropertyNames.Add(toExclude);
            return this;
        }
    }
}