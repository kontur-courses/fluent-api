using System;
using System.Collections;
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

        public readonly HashSet<object> printedObjects = new HashSet<object>();

        public readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        public readonly HashSet<string> excludedPropertyNames = new HashSet<string>();

        public readonly Dictionary<Type, TypeConfig<TOwner>> typeConfigs = new Dictionary<Type, TypeConfig<TOwner>>();

        public readonly Dictionary<string, IPropertyConfig> propertyConfigs = new Dictionary<string, IPropertyConfig>();

        public string PrintToString(TOwner obj)
        {
            printedObjects.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object printable, int nestingLevel)
        {
            if (printable == null)
                return "null" + Environment.NewLine;

            if (printedObjects.Contains(printable))
                return "Cyclic reference found." + Environment.NewLine;

            printedObjects.Add(printable);
            var printableType = printable.GetType();

            if (TypeHaveDifferentSerialization(printableType, out var typeConfig))
                return typeConfig.Func.Invoke(printable) + Environment.NewLine;

            if (finalTypes.Contains(printableType))
                return printable + Environment.NewLine;

            return ToStringComplexObject(printable, nestingLevel);
        }

        private bool TypeHaveDifferentSerialization(Type printableType, out TypeConfig<TOwner> typeConfig)
        {
            return typeConfigs.TryGetValue(printableType, out typeConfig) && typeConfig.Func != null;
        }

        private string ToStringComplexObject(object printable, int nestingLevel)
        {
            var printableType = printable.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(printableType.Name);
            foreach (var propertyInfo in printableType.GetProperties()
                .Where(x => !excludedTypes.Contains(x.PropertyType))
                .Where(x => !excludedPropertyNames.Contains(x.Name)))
            {
                if (PropertyHaveDifferentSerialization(propertyInfo, out var propertyConfig))
                {
                    sb.Append(ApplyAlternativeSerialization(printable, identation, propertyInfo, propertyConfig));
                }
                else
                {
                    if (propertyInfo.GetIndexParameters().Length > 0)
                    {
                        sb.Append(identation + "Elements:" + Environment.NewLine);
                        var innerIdentation = identation + '\t';
                        for (int i = 0; i < ((ICollection)printable).Count; i++)
                        {
                            sb.Append(innerIdentation + PrintToString(propertyInfo.GetValue(printable, new object[] { i }), nestingLevel + 1));
                        } 
                    }
                    else
                    {
                        sb.Append(ApplyDefaultSerialization(printable, nestingLevel, identation, propertyInfo));
                    } 
                }
            }

            return sb.ToString();
        }

        private string ApplyDefaultSerialization(object printable, int nestingLevel, string identation, PropertyInfo propertyInfo)
        {
            return identation + propertyInfo.Name + " = " +
                                      PrintToString(propertyInfo.GetValue(printable), nestingLevel + 1);
        }

        private static string ApplyAlternativeSerialization(object printable, string identation, PropertyInfo propertyInfo, IPropertyConfig propertyConfig)
        {
            return identation + propertyInfo.Name + " = " +
                                        propertyConfig.Func.Invoke(propertyInfo.GetValue(printable)) + Environment.NewLine;
        }

        private bool PropertyHaveDifferentSerialization(PropertyInfo propertyInfo, out IPropertyConfig propertyConfig)
        {
            return propertyConfigs.TryGetValue(propertyInfo.Name, out propertyConfig) && propertyConfig.Func != null;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public TypeConfig<TOwner> Printing<T>()
        {
            var config = new TypeConfig<TOwner>(this, typeof(T));
            typeConfigs.Add(config.Type, config);
            return config;
        }

        public PropertyConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> expression)
        {
            var config = new PropertyConfig<TOwner, T>(this, expression);
            propertyConfigs.Add(config.PropertyName, config);
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