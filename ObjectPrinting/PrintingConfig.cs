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
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }
            if (TryGetString(obj, nestingLevel, viewedObjects, out var resultString))
            {
                return resultString;
            }
            var typeCurrentObject = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(typeCurrentObject.Name);
            foreach (var propertyInfo in typeCurrentObject.GetProperties())
            {
                if (!viewedObjects.Add(propertyInfo.GetValue(obj)) && !finalTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (excludingTypes.Contains(propertyInfo.PropertyType) || excludingProperty.Contains(propertyInfo))
                    continue;
                if (specificPrintForProperty.TryGetValue(propertyInfo, out var specificPrint))
                    sb.AppendLine(identation + propertyInfo.Name + " = " + specificPrint(propertyInfo.GetValue(obj)));
                else
                    sb.Append(identation + propertyInfo.Name + " = " + PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, viewedObjects));
            }
            return sb.ToString();
        }

        private bool TryGetString(object obj, int nestingLevel, HashSet<object> viewedObjects, out string resultString)
        {
            resultString = string.Empty;
            var typeCurrentObject = obj.GetType();
            var haveSpecificPrintForCurrentType = specificPrintForType.TryGetValue(typeCurrentObject, out var printBySpecificRule);
            var currentObjectIsCollection = obj is ICollection;
            var currentTypeIsFinal = finalTypes.Contains(typeCurrentObject);
            if (!currentObjectIsCollection && !haveSpecificPrintForCurrentType && !currentTypeIsFinal)
                return false;
            if (haveSpecificPrintForCurrentType)
                resultString = printBySpecificRule(obj);
            else
                resultString = currentObjectIsCollection ? PrintCollectionsToString((ICollection)obj, nestingLevel, viewedObjects) : obj.ToString() + Environment.NewLine;
            return true;
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