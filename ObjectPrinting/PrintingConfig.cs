using System;
using System.Collections.Generic;
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

        private readonly Dictionary<Type, Delegate> TypesPrintOptions = new Dictionary<Type, Delegate>();

        //private readonly Dictionary<PropertyInfo, Delegate> PropertiesSerializationOptions = new Dictionary<PropertyInfo, Delegate>();
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
            TypesPrintOptions.Add(typeof(TPropType), printOption);
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
                return "null" + Environment.NewLine;

            var objectType = obj.GetType();

            if (!IsFinalType(objectType))
            {
                if (PrintedNonFinalObjects.Contains(obj))
                    return $"Cycled reference detected. Object <{objectType.Name}> doesn't printed" + Environment.NewLine;
                else
                    PrintedNonFinalObjects.Add(obj);
            }

            if (TypesPrintOptions.ContainsKey(objectType))
                return (string)TypesPrintOptions[objectType].DynamicInvoke(obj) + Environment.NewLine;

            if (IsFinalType(objectType))
                return obj + Environment.NewLine;

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
                    sb.Append(PrintPropertyWithOptions(obj, propertyInfo) + Environment.NewLine);
                    continue;
                }

                sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool IsFinalType(Type type)
        {
            return type.IsValueType || type == typeof(string);
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

        private string PrintPropertyWithOptions(object obj, PropertyInfo propertyInfo)
        {
            var propertyValue = propertyInfo.GetValue(obj);

            foreach (var printOption in PropertiesPrintOptions[propertyInfo])
                propertyValue = (string)printOption.DynamicInvoke(propertyValue);

            return (string)propertyValue;
        }
    }
}