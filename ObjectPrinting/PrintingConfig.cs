using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;
using System.Collections;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly ImmutableHashSet<Type> excludingTypes;
        private readonly ImmutableDictionary<Type, Func<object, string>> specificPrintForType;
        private readonly ImmutableDictionary<PropertyInfo, Func<object, string>> specificPrintForProperty;
        private readonly ImmutableHashSet<PropertyInfo> excludingProperty;
        private readonly int maxNestingLevel = 10;
        private readonly object[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludingTypes = ImmutableHashSet<Type>.Empty;
            specificPrintForType = ImmutableDictionary<Type, Func<object, string>>.Empty;
            excludingProperty = ImmutableHashSet<PropertyInfo>.Empty;
            specificPrintForProperty = ImmutableDictionary<PropertyInfo, Func<object, string>>.Empty;
        }

        private PrintingConfig(ImmutableHashSet<Type> excludingTypes, ImmutableDictionary<Type, Func<object, string>> specificPrintForType,
            ImmutableHashSet<PropertyInfo> excludingProperty, ImmutableDictionary<PropertyInfo, Func<object, string>> specificPrintForProperty, int maxNestingLevel)
        {
            this.excludingTypes = excludingTypes;
            this.specificPrintForType = specificPrintForType;
            this.excludingProperty = excludingProperty;
            this.specificPrintForProperty = specificPrintForProperty;
            this.maxNestingLevel = maxNestingLevel;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new  HashSet<object>{obj});
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> viewedObjects)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (specificPrintForType.ContainsKey(type))
            {
                return specificPrintForType[type].DynamicInvoke(obj) + Environment.NewLine;
            }
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            if (obj is ICollection){
                return  PrintCollectionsToString(obj, nestingLevel, viewedObjects);
            }
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
                if (!viewedObjects.Add(propertyInfo.GetValue(obj)) && !finalTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingProperty.Contains(propertyInfo))
                    continue;
                viewedObjects.Add(propertyInfo.GetValue(obj));
                if (specificPrintForProperty.TryGetValue(propertyInfo, out var specificPrint))
                {
                    sb.AppendLine(identation + propertyInfo.Name + " = " +
                              specificPrint(propertyInfo.GetValue(obj)));
                }
                else
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, viewedObjects));
                }
            }
            return sb.ToString();
        }

        private string PrintCollectionsToString(object obj, int nestingLevel, HashSet<object> viewedObjects){
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var collection = (IEnumerable)obj;
            foreach(var element in collection){
                sb.Append(identation + PrintToString(element, nestingLevel + 1, viewedObjects));
                viewedObjects.Add(element);
            }
            return sb.ToString();
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForType<TProperty>(Func<TProperty, string> specificSerialize)
        {
            var newCustomPrintForType = specificPrintForType.SetItem(typeof(TProperty), obj => specificSerialize((TProperty)obj));
            return new PrintingConfig<TOwner>(excludingTypes, newCustomPrintForType, excludingProperty, specificPrintForProperty, maxNestingLevel);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForProperty<TProperty>(Func<TProperty, string> specificSerialize, PropertyInfo property)
        {
            var newCustomPrintForProperty = specificPrintForProperty.SetItem(property, obj => specificSerialize((TProperty)obj));
            return new PrintingConfig<TOwner>(excludingTypes, specificPrintForType, excludingProperty, newCustomPrintForProperty, maxNestingLevel);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludingTypes = excludingTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(newExcludingTypes, specificPrintForType, excludingProperty, specificPrintForProperty, maxNestingLevel);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must return property.");
            }
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            var newExcludingProperty = excludingProperty.Add(propInfo);
            return new PrintingConfig<TOwner>(excludingTypes, specificPrintForType, newExcludingProperty, specificPrintForProperty, maxNestingLevel);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must return property.");
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
            return new PrintingConfig<TOwner>(excludingTypes, specificPrintForType, excludingProperty, specificPrintForProperty, maxLevel);
        }
    }
}