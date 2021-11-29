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
        private List<Type> excludedTypes = new List<Type>();

        private List<string> excludedPropertyNames = new List<string>();

        private List<TypeConfig<TOwner>> typeConfigs = new List<TypeConfig<TOwner>>();

        private List<IPropertyConfig> propertyConfigs = new List<IPropertyConfig>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            foreach (var config in typeConfigs)
            {
                if (config.type == obj.GetType())
                {
                    if (config.func != null)
                    {
                        return config.func.Invoke(obj) + Environment.NewLine;
                    }
                }
            }

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
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                bool isDone = false;
                bool isSkip = false;
                foreach (var name in excludedPropertyNames)
                {
                    if (propertyInfo.Name == name)
                    {
                        isSkip = true;
                        break;
                    }
                }
                if (isSkip)
                {
                    continue;
                }
                foreach (var config in propertyConfigs)
                {
                    if (config.propertyName == propertyInfo.Name)
                    {
                        sb.Append(identation + propertyInfo.Name + " = " +
                            config.func.Invoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                        isDone = true;
                    }
                }
                if (!excludedTypes.Contains(propertyInfo.PropertyType) && !isDone)
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
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
            typeConfigs.Add(config);
            return config;
        }

        public PropertyConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> expression)
        {
            var config = new PropertyConfig<TOwner, T>(this, expression);
            propertyConfigs.Add(config);
            return config;
        }

        public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
        {
            var strExpressionSplitted = expression.Body.ToString().Split('.');
            var toExclude = strExpressionSplitted[strExpressionSplitted.Length - 1];
            excludedPropertyNames.Add(toExclude);
            return this;
        }
    }
}