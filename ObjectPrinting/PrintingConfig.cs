using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<Type> excludingTypes;
        private readonly Dictionary<Type, Func<object, string>> customTypesPrints;

        Dictionary<Type, Func<object, string>> IPrintingConfig.CustomTypesPrints => customTypesPrints;

        private readonly List<string> excludingPropertys;
        private readonly Dictionary<string, Func<object, string>> customPropertysPrints;

        Dictionary<string, Func<object, string>> IPrintingConfig.CustomPropertysPrints => customPropertysPrints;

        public PrintingConfig()
        {
            excludingTypes = new List<Type>();
            customTypesPrints = new Dictionary<Type, Func<object, string>>();

            excludingPropertys = new List<string>();
            customPropertysPrints = new Dictionary<string, Func<object, string>>();
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
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (customTypesPrints.ContainsKey(propertyInfo.PropertyType))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                        customTypesPrints[propertyInfo.PropertyType](propertyInfo.GetValue(obj)));
                    continue;
                }
                if (customPropertysPrints.ContainsKey(propertyInfo.Name))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                        customPropertysPrints[propertyInfo.Name](propertyInfo.GetValue(obj)));
                    continue;
                }
                sb.Append(identation + propertyInfo.Name + " = " +
                    PrintToString(propertyInfo.GetValue(obj),
                    nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this, ((MemberExpression)func.Body).Member as PropertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            return this;
        }
    }

    public class PropertyPrintingConfig<TOwner, TProperty> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializationFunc)
        {
            if (propertyInfo == null)
                (parentConfig as IPrintingConfig).CustomTypesPrints.Add(
                    typeof(TProperty),
                    value => serializationFunc((TProperty)value));
            else
                (parentConfig as IPrintingConfig).CustomPropertysPrints.Add(
                    propertyInfo.Name,
                    value => serializationFunc((TProperty)value));
            return parentConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => this.parentConfig;
    }

    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo currentCulture)
        {
            ((config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig).CustomTypesPrints.Add(typeof(int), value => currentCulture.ToString());
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int length)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }
    }
}