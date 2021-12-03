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
        public readonly HashSet<Type> finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

        public readonly HashSet<object> PrintedObjects = new HashSet<object>();

        public readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        public readonly HashSet<string> excludedPropertyNames = new HashSet<string>();

        public readonly Dictionary<Type, TypeConfig<TOwner>> typeConfigs = new Dictionary<Type, TypeConfig<TOwner>>();

        public readonly Dictionary<string, IPropertyConfig> propertyConfigs = new Dictionary<string, IPropertyConfig>();


        public string PrintToString(TOwner obj)
        {
            var printer = new Printer<TOwner>(this);
            return printer.PrintToString(obj, 0);
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