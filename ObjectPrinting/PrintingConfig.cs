using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            var strObject = PrintToString(obj, 0);
            startObject = null!;
            return strObject;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            startObject ??= obj;

            if (obj == null)
                return "null" + Environment.NewLine;

            if (typesCustomSerializations.ContainsKey(obj.GetType()))
                return typesCustomSerializations[obj.GetType()](obj) + Environment.NewLine;

            if (obj.GetType().IsSimple())
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            IEnumerable properties;
            Func<object, object> valueExtractor;

            switch (obj)
            {
                case IDictionary dict:
                    properties = dict.Keys;
                    valueExtractor = (key) => dict[key];
                    break;
                case IEnumerable list:
                    properties = list
                        .Cast<object>()
                        .Select((e, index) => index);
                    valueExtractor = (index) => list.ElementAtOrDefault((int)index);
                    break;
                default:
                    properties = type.GetProperties();
                    valueExtractor = (propertyInfo) => (propertyInfo as PropertyInfo).GetValue(obj);
                    break;
            }

            foreach (var property in properties)
            {
                var value = valueExtractor(property);
                var propInfo = (property as PropertyInfo);
                if (excludedTypes.Contains(propInfo?.PropertyType)
                    || excludedNames.Contains(propInfo?.Name))
                    continue;

                sb.Append(identation);
                if (propInfo != null)
                    sb.Append(propInfo.Name);
                else 
                    sb.Append(property);
                sb.Append(" = ");

                if (value == startObject)
                    sb.Append("this" + Environment.NewLine);
                else if (propInfo != null && propsCustomSerializations.ContainsKey(propInfo.Name))
                    sb.Append(propsCustomSerializations[propInfo.Name](value) + Environment.NewLine);
                else
                    sb.Append(PrintToString(value, nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}