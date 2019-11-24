using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly ImmutableHashSet<Type> excludingTypes;
        private readonly ImmutableDictionary<Type, Delegate> customPrintForType;
        private readonly ImmutableDictionary<PropertyInfo, Delegate> customPrintForField;
        private readonly ImmutableHashSet<PropertyInfo> excludingFields;
        private readonly object[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludingTypes = ImmutableHashSet<Type>.Empty;
            customPrintForType = ImmutableDictionary<Type, Delegate>.Empty;
            excludingFields = ImmutableHashSet<PropertyInfo>.Empty;
            customPrintForField = ImmutableDictionary<PropertyInfo, Delegate>.Empty;
        }

        private PrintingConfig(ImmutableHashSet<Type> excludingTypes, ImmutableDictionary<Type, Delegate> customPrintForType,
            ImmutableHashSet<PropertyInfo> excludingFields, ImmutableDictionary<PropertyInfo, Delegate> customPrintForField)
        {
            this.excludingTypes = excludingTypes;
            this.customPrintForType = customPrintForType;
            this.excludingFields = excludingFields;
            this.customPrintForField = customPrintForField;
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
            if (customPrintForType.ContainsKey(obj.GetType()))
            {
                return customPrintForType[obj.GetType()].DynamicInvoke(obj) + Environment.NewLine;
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
                if (customPrintForField.TryGetValue(propertyInfo, out var result))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              result.DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);

                }
                else
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
                }
            }
            return sb.ToString();
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForType<TProperty>(Func<TProperty, string> func)
        {
            var newCustomPrintForType = this.customPrintForType.SetItem(typeof(TProperty), func);
            return new PrintingConfig<TOwner>(this.excludingTypes, newCustomPrintForType, this.excludingFields, this.customPrintForField);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForField<TProperty>(Func<TProperty, string> func, PropertyInfo property)
        {
            var newCustomPrintForField = this.customPrintForField.SetItem(property, func);
            return new PrintingConfig<TOwner>(this.excludingTypes, this.customPrintForType, this.excludingFields, newCustomPrintForField);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludingTypes = excludingTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(newExcludingTypes, this.customPrintForType, this.excludingFields, this.customPrintForField);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must return field.");
            }
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            var newExcludingFields = excludingFields.Add(propInfo);
            return new PrintingConfig<TOwner>(excludingTypes, customPrintForType, newExcludingFields, this.customPrintForField);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must return field.");
            }
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, propInfo);
        }
    }
}