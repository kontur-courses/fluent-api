using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly ImmutableHashSet<Type> excludingTypes;
        private readonly ImmutableDictionary<Type, Delegate> customPrint;
        private readonly ImmutableHashSet<PropertyInfo> excludingFields;
        private readonly object[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludingTypes = ImmutableHashSet<Type>.Empty;
            customPrint = ImmutableDictionary<Type, Delegate>.Empty;
            excludingFields = ImmutableHashSet<PropertyInfo>.Empty;
        }

        private PrintingConfig(ImmutableHashSet<Type> excludingTypes, ImmutableDictionary<Type, Delegate> customPrint,
            ImmutableHashSet<PropertyInfo> excludingFields)
        {
            this.excludingTypes = excludingTypes;
            this.customPrint = customPrint;
            this.excludingFields = excludingFields;
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
            if (customPrint.ContainsKey(obj.GetType()))
            {
                return customPrint[obj.GetType()].DynamicInvoke(obj) + Environment.NewLine;
            }
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingFields.Contains(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrint<TProperty>(Func<TProperty, string> func)
        {
            var newCustomPrint = this.customPrint.SetItem(typeof(TProperty), func);
            return new PrintingConfig<TOwner>(this.excludingTypes, newCustomPrint, this.excludingFields);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludingTypes = excludingTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(newExcludingTypes, this.customPrint, this.excludingFields);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            var newExcludingFields = excludingFields.Add(propInfo);
            return new PrintingConfig<TOwner>(excludingTypes, customPrint, newExcludingFields);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }
    }
}