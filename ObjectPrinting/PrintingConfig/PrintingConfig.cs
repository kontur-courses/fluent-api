using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting.PrintingConfig
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<string> excludedProperties;
        private readonly Dictionary<Type, Func<object, string>> typesPrintingMethods;
        private readonly Dictionary<string, Func<object, string>> propertiesPrintingMethods;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };


        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypesPrintingMethods
            => typesPrintingMethods;

        Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.PropertiesPrintingMethods
            => propertiesPrintingMethods;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<string>();
            typesPrintingMethods = new Dictionary<Type, Func<object, string>>();
            propertiesPrintingMethods = new Dictionary<string, Func<object, string>>();
        }

        public PrintingConfig<TOwner> ExcludingProperty<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            excludedProperties.Add(GetPropertyName(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> ExcludingPropertyWithType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> ExcludingPropertyWithTypes(params Type[] types)
        {
            excludedTypes.UnionWith(types);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> PrintProperty<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyName(memberSelector));
        }

        public PropertyPrintingConfig<TOwner, TPropType> PrintProperty<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private string PrintToString(object obj, int nestingLevel,
            HashSet<object> printedObjects, PropertyInfo propertyInfo = null)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (printedObjects.Contains(obj))
                return "Loopback detected" + Environment.NewLine;

            var type = obj.GetType();
            var sb = new StringBuilder();
            var objectPrinting = GetPrintedObject(obj, type, propertyInfo);
            sb.Append(objectPrinting + Environment.NewLine);

            if (type.IsClass)
                printedObjects.Add(obj);
            if (finalTypes.Contains(type))
                return objectPrinting + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            if (obj is IEnumerable enumerable)
                sb.Append(GetPrintedEnumerableObj(enumerable, nestingLevel, printedObjects));
            else
                foreach (var propInfo in type.GetProperties())
                {
                    if (excludedTypes.Contains(propInfo.PropertyType)
                        || excludedProperties.Contains(propInfo.Name)) continue;
                    var printedObject = PrintToString(
                        propInfo.GetValue(obj), nestingLevel + 1, printedObjects, propInfo);
                    sb.Append($"{indentation}{propInfo.Name} = {printedObject}");
                }

            return sb.ToString();
        }

        private string GetPrintedObject(object obj, Type type, PropertyInfo propertyInfo)
        {
            var objectPrinting = type.Name;
            var propertyName = propertyInfo?.Name;
            if (typesPrintingMethods.TryGetValue(type, out var printMethod))
                return printMethod(obj);
            if (propertyInfo != null &&
                propertiesPrintingMethods.TryGetValue(propertyName, out printMethod))
                return printMethod(obj);
            if (finalTypes.Contains(type))
                return obj.ToString();
            return objectPrinting;
        }

        private string GetPrintedEnumerableObj(IEnumerable obj, int nestingLevel, HashSet<object> printedObjects)
        {
            var sb = new StringBuilder();
            if (obj is IDictionary dict)
                AddPrintingDictPairs(dict, sb, nestingLevel, printedObjects);
            else
                AddPrintingSequenceElems(obj, sb, nestingLevel, printedObjects);

            return sb.ToString();
        }

        private void AddPrintingSequenceElems(IEnumerable obj, StringBuilder sb, 
            int nestingLevel, HashSet<object> printedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var count = 0;
            foreach (var element in obj)
                sb.Append($"{indentation}{count++}: " +
                          $"{PrintToString(element, nestingLevel + 1, printedObjects)}");
        }

        private void AddPrintingDictPairs(IDictionary dict, StringBuilder sb, 
            int nestingLevel, HashSet<object> printedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);
            foreach (DictionaryEntry pair in dict)
                sb.Append(
                    $"{indentation}{pair.Key}: " +
                    $"{PrintToString(pair.Value, nestingLevel + 1, printedObjects)}");
        }

        private string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo =
                ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            return propertyInfo?.Name;
        }
    }
}