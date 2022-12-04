using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> ExcludedPropTypes = new HashSet<Type>();

        private readonly HashSet<PropertyInfo> ExcludedProperty = new HashSet<PropertyInfo>();

        private readonly Dictionary<Type, List<Delegate>> TypesPrintOptions = new Dictionary<Type, List<Delegate>>();

        private readonly Dictionary<PropertyInfo, List<Delegate>> PropertiesPrintOptions = new Dictionary<PropertyInfo, List<Delegate>>();

        private readonly HashSet<object> PrintedNonFinalObjects = new HashSet<object>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = GetPropertyInfo(memberSelector);

            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludePropertyType(typeof(TPropType));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = GetPropertyInfo(memberSelector);

            ExcludeProperty(propertyInfo);

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public void AddTypePrintOption<TPropType>(Type type, Func<TPropType, string> printOption)
        {
            if (!TypesPrintOptions.ContainsKey(type))
                TypesPrintOptions.Add(type, new List<Delegate> { printOption });
            else
                TypesPrintOptions[type].Add(printOption);
        }

        public void AddPropertyPrintOption<TPropType>(PropertyInfo propertyInfo, Func<TPropType, string> printOption)
        {
            if (!PropertiesPrintOptions.ContainsKey(propertyInfo))
                PropertiesPrintOptions.Add(propertyInfo, new List<Delegate> { printOption });
            else
                PropertiesPrintOptions[propertyInfo].Add(printOption);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (obj is IList list)
                return PrintIListCollection(list, nestingLevel);

            if (obj is IDictionary dictionary)
                return PrintIDictionaryCollection(dictionary, nestingLevel);

            var objectType = obj.GetType();

            if (!IsFinalType(objectType))
            {
                if (PrintedNonFinalObjects.Contains(obj))
                    return $"Cycled reference detected. Object <{objectType.Name}> doesn't printed";
                else
                    PrintedNonFinalObjects.Add(obj);
            }

            if (HasPrintOption(objectType))
                return PrintTypeWithOptions(obj);

            if (IsFinalType(objectType))
                return obj.ToString();

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objectType.Name);

            foreach (var propertyInfo in objectType.GetProperties())
            {
                if (IsExcluded(propertyInfo))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = ");

                if (HasPrintOption(propertyInfo))
                {
                    var printedPropertyValue = PrintPropertyWithOptions(obj, propertyInfo);

                    sb.Append(PrintStringTypeWithOptions(printedPropertyValue) + Environment.NewLine);
                    continue;
                }

                sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1) + Environment.NewLine);
            }
            return sb.ToString();
        }

        private bool IsFinalType(Type type)
        {
            var implementedInterfaces = type.GetInterfaces();

            return (implementedInterfaces.Contains(typeof(IConvertible)) ||
                    implementedInterfaces.Contains(typeof(IFormattable)));
        }

        private PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;

            return member as PropertyInfo;
        }

        private void ExcludePropertyType(Type propertyType)
        {
            ExcludedPropTypes.Add(propertyType);
        }

        private void ExcludeProperty(PropertyInfo propertyInfo)
        {
            ExcludedProperty.Add(propertyInfo);
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return ExcludedPropTypes.Contains(propertyInfo.PropertyType) || ExcludedProperty.Contains(propertyInfo);
        }

        private bool HasPrintOption(PropertyInfo propertyInfo)
        {
            return PropertiesPrintOptions.ContainsKey(propertyInfo);
        }

        private bool HasPrintOption(Type type)
        {
            return TypesPrintOptions.ContainsKey(type);
        }

        private string PrintPropertyWithOptions(object obj, PropertyInfo propertyInfo)
        {
            var propertyValue = propertyInfo.GetValue(obj);

            foreach (var printOption in PropertiesPrintOptions[propertyInfo])
                propertyValue = (string)printOption.DynamicInvoke(propertyValue);

            return (string)propertyValue;
        }

        private string PrintStringTypeWithOptions(string inputString)
        {
            if (HasPrintOption(typeof(string)))
                return PrintTypeWithOptions(inputString);

            return inputString;
        }

        private string PrintTypeWithOptions(object obj)
        {
            var printedObject = obj;

            foreach (var printOption in TypesPrintOptions[obj.GetType()])
                printedObject = (string)printOption.DynamicInvoke(printedObject);

            return (string)printedObject;
        }

        private string PrintIListCollection(IList list, int nestingLevel)
        {
            var sb = new StringBuilder();

            var identation = new string('\t', nestingLevel + 1);

            sb.Append("\n")
              .Append(identation)
              .Append("[ ");

            foreach (var item in list)
                sb.Append("\n" + identation + PrintToString(item, identation.Length + 1));

            sb.Append("\n")
              .Append(identation)
              .Append("]");

            return sb.ToString();
        }

        private string PrintIDictionaryCollection(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();

            var identation = new string('\t', nestingLevel + 1);

            foreach (var item in dictionary)
            {
                var dictionaryItem = (DictionaryEntry)item;

                sb.Append("\n")
                  .Append(identation)
                  .Append("[ ")
                  .Append(PrintToString(dictionaryItem.Key, identation.Length))
                  .Append(" ] = ")
                  .Append(PrintToString(dictionaryItem.Value, identation.Length));
            }

            return sb.ToString();
        }
    }
}