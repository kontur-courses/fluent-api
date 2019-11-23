using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private List<SerializationRule> SerializationRules;

        public PrintingConfig()
        {
            SerializationRules = new List<SerializationRule>();
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
                string res = null;
                var isSelected = true;
                foreach (var serializationRule in SerializationRules)
                {
                    isSelected &= serializationRule.filterHandler.Invoke(obj, propertyInfo);
                    if (!isSelected)
                        break;
                    res = identation + propertyInfo.Name + " = " +
                          serializationRule.resultHandler.Invoke(obj, propertyInfo, identation, nestingLevel + 1);
                }

                if (!isSelected)
                    continue;
                sb.Append(res ?? DefaultResultHandler(obj, propertyInfo, identation, nestingLevel));
            }

            return sb.ToString();
        }

        string DefaultResultHandler(object obj, PropertyInfo propertyInfo, string identation, int nestingLevel)
        {
            return identation + propertyInfo.Name + " = " +
                   PrintToString(propertyInfo.GetValue(obj),
                       nestingLevel + 1);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            SerializationRules.Add(
                new SerializationRule((obj, propertyInfo) => propertyInfo.PropertyType is T,
                DefaultResultHandler));
            return this;
        }

        public PrintingConfig<TOwner> Excluding(Expression<Func<TOwner, string>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;

            SerializationRules.Add(
                new SerializationRule((obj, propertyInfo) => propertyInfo.Name == propInfo?.Name,
                DefaultResultHandler));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            throw new NotImplementedException();
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> func)
        {
            throw new NotImplementedException();
        }
    }
}