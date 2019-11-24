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
        private HashSet<Type> ExcludedTypes;
        private HashSet<string> ExcludedProperties;
        private Dictionary<Type, Func<object, string>> SerializeFunctions;

        public PrintingConfig()
        {
            ExcludedTypes = new HashSet<Type>();
            SerializeFunctions = new Dictionary<Type, Func<object, string>>();
            ExcludedProperties = new HashSet<string>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
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
            Func<object, string> func = x => print((TPropType)x);
            SerializeFunctions.Add(typeof(TPropType), func);
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
                if (ExcludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (ExcludedProperties.Contains(propertyInfo.Name))
                    continue;
                if (SerializeFunctions.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              SerializeFunctions[propertyInfo.PropertyType].Invoke(propertyInfo.GetValue(obj))
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