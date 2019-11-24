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
        private readonly ImmutableDictionary<Type, Func<object, string>> customPrintForType;
        private readonly ImmutableDictionary<PropertyInfo, Func<object, string>> customPrintForField;
        private readonly ImmutableHashSet<PropertyInfo> excludingFields;
        private readonly int maxNestingLevel = 10;
        private readonly object[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludingTypes = ImmutableHashSet<Type>.Empty;
            customPrintForType = ImmutableDictionary<Type, Func<object, string>>.Empty;
            excludingFields = ImmutableHashSet<PropertyInfo>.Empty;
            customPrintForField = ImmutableDictionary<PropertyInfo, Func<object, string>>.Empty;
        }

        private PrintingConfig(ImmutableHashSet<Type> excludingTypes, ImmutableDictionary<Type, Func<object, string>> customPrintForType,
            ImmutableHashSet<PropertyInfo> excludingFields, ImmutableDictionary<PropertyInfo, Func<object, string>> customPrintForField, int maxNestingLevel)
        {
            this.excludingTypes = excludingTypes;
            this.customPrintForType = customPrintForType;
            this.excludingFields = excludingFields;
            this.customPrintForField = customPrintForField;
            this.maxNestingLevel = maxNestingLevel;
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
            var type = obj.GetType();
            if (customPrintForType.ContainsKey(type))
            {
                return customPrintForType[type].DynamicInvoke(obj) + Environment.NewLine;
            }
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var nextNestingLevel = nestingLevel + 1;
            if (nextNestingLevel > maxNestingLevel)
            {
                sb.AppendLine(identation + "Out of range max nesting level.");
                return sb.ToString();
            }
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingFields.Contains(propertyInfo))
                    continue;
                if (customPrintForField.TryGetValue(propertyInfo, out var customPrint))
                {
                    sb.AppendLine(identation + propertyInfo.Name + " = " +
                              customPrint.DynamicInvoke(propertyInfo.GetValue(obj)));
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
            var newCustomPrintForType = customPrintForType.SetItem(typeof(TProperty), obj => func((TProperty)obj));
            return new PrintingConfig<TOwner>(excludingTypes, newCustomPrintForType, excludingFields, customPrintForField, maxNestingLevel);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForProperty<TProperty>(Func<TProperty, string> func, PropertyInfo property)
        {
            var newCustomPrintForField = customPrintForField.SetItem(property, obj => func((TProperty)obj));
            return new PrintingConfig<TOwner>(excludingTypes, customPrintForType, excludingFields, newCustomPrintForField, maxNestingLevel);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludingTypes = excludingTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(newExcludingTypes, customPrintForType, excludingFields, customPrintForField, maxNestingLevel);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must return field.");
            }
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            var newExcludingFields = excludingFields.Add(propInfo);
            return new PrintingConfig<TOwner>(excludingTypes, customPrintForType, newExcludingFields, customPrintForField, maxNestingLevel);
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

        public PrintingConfig<TOwner> SetMaxNestingLevel(int maxLevel)
        {
            if (maxLevel < 0)
            {
                throw new ArgumentException("Nesting level must be positive number.");
            }
            return new PrintingConfig<TOwner>(excludingTypes, customPrintForType, excludingFields, customPrintForField, maxLevel);
        }
    }
}