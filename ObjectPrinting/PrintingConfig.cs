using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private object startObject;
        private HashSet<Type> excludedTypes;
        private HashSet<string> excludedNames;
        private Dictionary<Type, Func<object, string>> typesCustomSerializations;
        private Dictionary<string, Func<object, string>> propsCustomSerializations;

        public PrintingConfig(HashSet<Type> types = null, HashSet<string> names = null)
        {
            excludedTypes = types ?? new HashSet<Type>();
            excludedNames = names ?? new HashSet<string>();
            typesCustomSerializations = new Dictionary<Type, Func<object, string>>();
            propsCustomSerializations = new Dictionary<string, Func<object, string>>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> For<TPropType>()
        {
            var config = new PropertyPrintingConfig<TOwner, TPropType>(this);
            typesCustomSerializations.Add(typeof(TPropType), obj => config.GetProperty(obj));
            return config;
        }

        public PropertyPrintingConfig<TOwner, TPropType> For<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            var config = new PropertyPrintingConfig<TOwner, TPropType>(this);
            var prop = (selector.Body as MemberExpression).Member as PropertyInfo;
            propsCustomSerializations.Add(prop.Name, obj => config.GetProperty(obj));
            return config;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            var prop = (selector.Body as MemberExpression).Member as PropertyInfo;
            excludedNames.Add(prop.Name);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            startObject ??= obj;

            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (typesCustomSerializations.ContainsKey(obj.GetType()))
                return typesCustomSerializations[obj.GetType()](obj) + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);


            if (obj is IDictionary dict)
            {
                foreach (var key in dict.Keys)
                {
                    sb.Append(identation);
                    sb.Append(key);
                    sb.Append(" = ");
                    sb.Append(PrintToString(dict[key], nestingLevel + 1));
                }

                return sb.ToString();
            }

            if (obj is IEnumerable)
            {
                var i = 0;
                foreach (var e in obj as IEnumerable)
                {
                    sb.Append(identation);
                    sb.Append(i);
                    sb.Append(" = ");
                    sb.Append(PrintToString(e, nestingLevel + 1));
                    i++;
                }
                return sb.ToString();
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)
                    || excludedNames.Contains(propertyInfo.Name)) 
                    continue;

                sb.Append(identation);
                sb.Append(propertyInfo.Name + " = ");

                if (propertyInfo.GetValue(obj) == startObject)
                    sb.Append("this" + Environment.NewLine);
                else if (propsCustomSerializations.ContainsKey(propertyInfo.Name))
                    sb.Append(propsCustomSerializations[propertyInfo.Name](propertyInfo.GetValue(obj)) +
                              Environment.NewLine);
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));

            }
            return sb.ToString();
        }
    }
}