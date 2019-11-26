using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Collections;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly ImmutableHashSet<Type> excludingTypes;
        private readonly ImmutableDictionary<Type, Func<object, string>> specificPrintForType;
        private readonly ImmutableDictionary<PropertyInfo, Func<object, string>> specificPrintForProperty;
        private readonly ImmutableHashSet<PropertyInfo> excludingProperty;
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
            ImmutableHashSet<PropertyInfo> excludingProperty, ImmutableDictionary<PropertyInfo, Func<object, string>> specificPrintForProperty)
        {
            this.excludingTypes = excludingTypes;
            this.specificPrintForType = specificPrintForType;
            this.excludingProperty = excludingProperty;
            this.specificPrintForProperty = specificPrintForProperty;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new  HashSet<object>{obj});
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> viewedObjects)
        {
            if (TryGetString(obj, nestingLevel, viewedObjects, out var resultString))
            {
                return resultString;
            }
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!viewedObjects.Add(propertyInfo.GetValue(obj)) && !finalTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingProperty.Contains(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " + (specificPrintForProperty.TryGetValue(propertyInfo, out var specificPrint)
                    ? specificPrint(propertyInfo.GetValue(obj)) + Environment.NewLine : PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, viewedObjects)));
            }
            return sb.ToString();
        }

        private bool TryGetString(object obj, int nestingLevel, HashSet<object> viewedObjects, out string resultString)
        {
            if (obj == null)
            {
                resultString = "null" + Environment.NewLine;
                return true;
            }
            var type = obj.GetType();
            if (specificPrintForType.ContainsKey(type))
            {
                resultString = specificPrintForType[type].DynamicInvoke(obj) + Environment.NewLine;
                return true;
            }
            if (finalTypes.Contains(type))
            {
                resultString = obj + Environment.NewLine;
                return true;
            }
            if (obj is ICollection collection)
            {
                resultString = PrintCollectionsToString(collection, nestingLevel, viewedObjects);
                return true;
            }
            resultString = null;
            return false;
        }

        private string PrintCollectionsToString(ICollection collection, int nestingLevel, HashSet<object> viewedObjects)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(collection.GetType().Name);
            foreach(var element in collection){
                if (!viewedObjects.Add(element) && !finalTypes.Contains(element.GetType()))
                    continue;
                sb.Append(identation + PrintToString(element, nestingLevel + 1, viewedObjects));
            }
            return sb.ToString();
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForType<TProperty>(Func<TProperty, string> specificSerialize)
        {
            var newCustomPrintForType = specificPrintForType.SetItem(typeof(TProperty), obj => specificSerialize((TProperty)obj));
            return new PrintingConfig<TOwner>(excludingTypes, newCustomPrintForType, excludingProperty, specificPrintForProperty);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPrintForProperty<TProperty>(Func<TProperty, string> specificSerialize, PropertyInfo property)
        {
            var newCustomPrintForProperty = specificPrintForProperty.SetItem(property, obj => specificSerialize((TProperty)obj));
            return new PrintingConfig<TOwner>(excludingTypes, specificPrintForType, excludingProperty, newCustomPrintForProperty);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var newExcludingTypes = excludingTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(newExcludingTypes, specificPrintForType, excludingProperty, specificPrintForProperty);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must be MemberExpression and return Property.");
            }
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            var newExcludingProperty = excludingProperty.Add(propInfo);
            return new PrintingConfig<TOwner>(excludingTypes, specificPrintForType, newExcludingProperty, specificPrintForProperty);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            if (!(func.Body is MemberExpression expression && expression.Member is PropertyInfo))
            {
                throw new ArgumentException("Function must be MemberExpression and return Property.");
            }
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, propInfo);
        }
    }
}