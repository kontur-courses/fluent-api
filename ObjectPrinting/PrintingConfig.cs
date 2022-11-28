using System;
using System.Collections.Generic;
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
        private HashSet<string> excludedTypes;
        private HashSet<string> excludedNames;

        public PrintingConfig(HashSet<string> types, HashSet<string> names)
        {
            excludedTypes = types == null ? new HashSet<string>() : types;
            excludedNames = names == null ? new HashSet<string>() : names;
        }

        public PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>(excludedTypes, excludedNames);
        }

        public PrintingConfig<TOwner> SetSerialization(Action<TOwner> action)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> SetSerialization<T>()
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> Exclude<T>()
        { 
            excludedTypes.Add(typeof(T).Name);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> func)
        {
            var prop = (func.Body as MemberExpression).Member as PropertyInfo;
            excludedNames.Add(prop.Name);
            return this;
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
                if (excludedTypes.Contains(propertyInfo.PropertyType.Name)
                    && excludedNames.Contains(propertyInfo.Name)) 
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}