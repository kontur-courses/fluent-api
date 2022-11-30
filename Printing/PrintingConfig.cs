using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(bool), typeof(sbyte),  typeof(byte),  typeof(short),  typeof(ushort),
            typeof(int),  typeof(uint),  typeof(long), typeof(ulong), typeof(float),
            typeof(double),  typeof(decimal),  typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> ExcludedPropTypes = new HashSet<Type>();

        private readonly HashSet<PropertyInfo> ExcludedProperty = new HashSet<PropertyInfo>();

        private readonly Dictionary<Type, Delegate> TypesSerializationOptions = new Dictionary<Type, Delegate>();

        private readonly Dictionary<PropertyInfo, Delegate> PropertiesSerializationOptions = new Dictionary<PropertyInfo, Delegate>();

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

        public void AddTypeSerializationOption<TPropType>(Type type, Func<TPropType, string> printOption)
        {
            TypesSerializationOptions.Add(typeof(TPropType), printOption);
        }

        public void AddPropertySerializationOption<TPropType>(PropertyInfo propertyInfo, Func<TPropType, string> printOption)
        {
            PropertiesSerializationOptions.Add(propertyInfo, printOption);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objectType = obj.GetType();

            if (FinalTypes.Contains(objectType))
            {
                if (PrintedNonFinalObjects.Contains(obj))
                    throw new InvalidOperationException($"Object {objectType} has cycled reference");
                else
                    PrintedNonFinalObjects.Add(obj);
            }

            if (TypesSerializationOptions.ContainsKey(objectType))
                return (string)TypesSerializationOptions[objectType].DynamicInvoke(obj) + Environment.NewLine;

            if (FinalTypes.Contains(objectType))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objectType.Name);

            foreach (var propertyInfo in objectType.GetProperties())
            {
                if (IsExcluded(propertyInfo))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = ");

                if (PropertiesSerializationOptions.ContainsKey(propertyInfo))
                {
                    sb.Append((string)PropertiesSerializationOptions[propertyInfo].DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
            return sb.ToString();
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
    }
}