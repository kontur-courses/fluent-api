using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions.Common;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> ExcludedTypes;
        private readonly HashSet<string> ExcludedProperties;
        private readonly Dictionary<Type, Func<object, string>> SerializeFunctionsForTypes;
        private readonly Dictionary<string, Func<object, string>> SerializeFunctionsForProperties;
        private readonly HashSet<object> AlreadySerialized;

        public PrintingConfig()
        {
            ExcludedTypes = new HashSet<Type>();
            SerializeFunctionsForTypes = new Dictionary<Type, Func<object, string>>();
            SerializeFunctionsForProperties = new Dictionary<string, Func<object, string>>();
            ExcludedProperties = new HashSet<string>();
            AlreadySerialized = new HashSet<object>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var a = memberSelector.Body as MemberExpression;
            var b = a.Member.Name;
            ExcludedProperties.Add(b);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public void SetSerialization<TPropType>(Func<TPropType, string> print)
        {
            SerializeFunctionsForTypes.Add(typeof(TPropType), x => print((TPropType)x));
        }

        public void SetSerialization<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector, Func<TPropType, string> print)
        {
            var a = memberSelector.Body as MemberExpression;
            var b = a.Member.Name;
            Func<object, string> func = x => print((TPropType)x);
            SerializeFunctionsForProperties.Add(b, func);
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

            AlreadySerialized.Add(obj);
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (ExcludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (ExcludedProperties.Contains(propertyInfo.Name))
                    continue;

                if(AlreadySerialized.Contains(propertyInfo.GetValue(obj)))
                    continue;

                if (SerializeFunctionsForProperties.ContainsKey(propertyInfo.Name))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              SerializeFunctionsForProperties[propertyInfo.Name].Invoke(propertyInfo.GetValue(obj))
                              + Environment.NewLine);
                else if (SerializeFunctionsForTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              SerializeFunctionsForTypes[propertyInfo.PropertyType].Invoke(propertyInfo.GetValue(obj))
                              + Environment.NewLine);
                else
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}