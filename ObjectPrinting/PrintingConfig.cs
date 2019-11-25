using System;
using System.Collections.Generic;
using System.Collections;
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

        private int maxNumberListItems;

        public PrintingConfig()
        {
            excludingTypes = new List<Type>();
            customTypesPrints = new Dictionary<Type, Func<object, string>>();

            excludingPropertys = new List<string>();
            customPropertysPrints = new Dictionary<string, Func<object, string>>();

            maxNumberListItems = -1;
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
            if (obj is IEnumerable)
            {
                return PrintToStringForIEnumerable((IEnumerable)obj, nestingLevel + 1);
            }

            return PrintToStringForProperty(obj, nestingLevel + 1);
        }

        private string PrintToStringForProperty(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType) ||
                    excludingPropertys.Contains(propertyInfo.Name))
                    continue;
                var str = "";
                if (customTypesPrints.ContainsKey(propertyInfo.PropertyType))
                {
                    str = customTypesPrints[propertyInfo.PropertyType](propertyInfo.GetValue(obj));
                }
                else if (customPropertysPrints.ContainsKey(propertyInfo.Name))
                {
                    str = customPropertysPrints[propertyInfo.Name](propertyInfo.GetValue(obj));
                }
                else
                {
                    str = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                }
                sb.Append(identation + propertyInfo.Name + " = " + str);
            }
            return sb.ToString();
        }

        private string PrintToStringForIEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = enumerable.GetType();
            sb.AppendLine(type.Name);
            var count = 0;
            foreach (var item in enumerable)
            {
                sb.Append(identation + PrintToString(item, nestingLevel + 1));
                count++;
                if (count >= maxNumberListItems && maxNumberListItems >= 0)
                    break;
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
            excludingPropertys.Add((((MemberExpression)func.Body).Member as PropertyInfo).Name);
            return this;
        }

        public PrintingConfig<TOwner> SetMaxNumberListItems(int maxNumberListItems)
        {
            this.maxNumberListItems = maxNumberListItems;
            return this;
        }
    }
}