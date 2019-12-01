﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : PrintingConfigBase, IPrintingConfig
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        private readonly Dictionary<Type, Func<object, string>> alternatePropertySerialisator =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> cultureInfoApplierByType =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<PropertyInfo, Func<object, string>> individualSetUpFuncByPropertyInfo =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly Dictionary<PropertyInfo, int> maxValueLengthByPropertyInfo =
            new Dictionary<PropertyInfo, int>();

        private int mutualMaxPropertiesLength;

        private PropertyInfo currentSettingUpProperty;

        void IPrintingConfig.SetCultureInfoApplierForNumberType<TNumber>(Func<TNumber, string> cultureInfoApplier)
        {
            if (currentSettingUpProperty is null)
                cultureInfoApplierByType[typeof(TNumber)] = ApplyCultureInfo;
            else
                individualSetUpFuncByPropertyInfo[currentSettingUpProperty] = ApplyCultureInfo;

            string ApplyCultureInfo(object number) => cultureInfoApplier((TNumber)number);
        }

        void IPrintingConfig.SetMaxValueLengthForStringProperty(int maxValueLength)
        {
            if (currentSettingUpProperty is null)
                mutualMaxPropertiesLength = maxValueLength;
            else
                maxValueLengthByPropertyInfo[currentSettingUpProperty] = maxValueLength;
        }

        void IPrintingConfig.SetAlternatePropertySerialisator<TPropType>(Func<TPropType, string> alternateSerialisator)
        {
            if (currentSettingUpProperty is null)
                alternatePropertySerialisator[typeof(TPropType)] = SerialiseProperty;
            else
                individualSetUpFuncByPropertyInfo[currentSettingUpProperty] = SerialiseProperty;

            string SerialiseProperty(object propertyValue) => alternateSerialisator((TPropType)propertyValue);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            currentSettingUpProperty = null;

            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentSettingUpProperty = GetPropertyInfoFromMemberExpression(memberSelector);

            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = GetPropertyInfoFromMemberExpression(memberSelector);

            excludedProperties.Add(propertyInfo);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public string PrintToString(TOwner printedObject) => PrintToString(printedObject, 0);

        private string PrintToString(object printedObject, int nestingLevel)
        {
            if (printedObject is null)
                return NullRepresentation + Environment.NewLine;

            var objectRuntimeType = printedObject.GetType();

            if (excludedTypes.Contains(objectRuntimeType))
                return string.Empty;

            if (alternatePropertySerialisator.ContainsKey(objectRuntimeType))
                return alternatePropertySerialisator[objectRuntimeType](printedObject) + Environment.NewLine;

            if (cultureInfoApplierByType.ContainsKey(objectRuntimeType))
                return cultureInfoApplierByType[objectRuntimeType](printedObject) + Environment.NewLine;

            if (FinalTypes.Contains(objectRuntimeType))
                return printedObject + Environment.NewLine;

            var objectSerialisationBuilder = new StringBuilder();

            objectSerialisationBuilder.AppendLine(objectRuntimeType.Name);

            objectSerialisationBuilder.Append(PrintAllProperties(printedObject, objectRuntimeType, nestingLevel));

            return objectSerialisationBuilder.ToString();
        }

        private string PrintAllProperties(object printedObject, Type objectRuntimeType, int nestingLevel)
        {
            var propertiesBuilder = new StringBuilder();
            var indentation = new string(Indentation, nestingLevel + 1);

            foreach (var propertyInfo in objectRuntimeType.GetProperties().Where(pInfo => !IsExcludedProperty(pInfo)))
            {
                var propertyValue = propertyInfo.GetValue(printedObject);

                var propertyValueSerialisation = SerialiseValue(propertyInfo, propertyValue);

                propertyValueSerialisation = TrimValueIfNecessary(propertyInfo, propertyValueSerialisation);

                var propertySerialisation = string.Concat(indentation,
                                                          propertyInfo.Name,
                                                          " = ",
                                                          propertyValueSerialisation);

                propertiesBuilder.Append(propertySerialisation);
            }

            return propertiesBuilder.ToString();

            string SerialiseValue(PropertyInfo propertyInfo, object propertyValue) =>
                individualSetUpFuncByPropertyInfo.ContainsKey(propertyInfo)
                    ? individualSetUpFuncByPropertyInfo[propertyInfo](propertyValue) + Environment.NewLine
                    : PrintToString(propertyValue, nestingLevel + 1);

            string TrimValueIfNecessary(PropertyInfo propertyInfo, string propertyValueSerialisation)
            {
                if (propertyInfo.PropertyType != typeof(string)) return propertyValueSerialisation;

                var maxPropertyLength = maxValueLengthByPropertyInfo.ContainsKey(propertyInfo)
                                            ? maxValueLengthByPropertyInfo[propertyInfo]
                                            : mutualMaxPropertiesLength;

                if (maxPropertyLength > 0)
                    return TruncateString(propertyValueSerialisation, maxPropertyLength) + $"...{Environment.NewLine}";

                return propertyValueSerialisation;
            }
        }

        private bool IsExcludedProperty(PropertyInfo propertyInfo) =>
            excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo);

        private static PropertyInfo GetPropertyInfoFromMemberExpression<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression) ||
                !(memberExpression.Member is PropertyInfo propertyInfo))
                throw new ArgumentException("Passed expression has to represent accessing a property",
                                            nameof(memberSelector));
            return propertyInfo;
        }

        private static string TruncateString(string str, int maxLength) =>
            string.IsNullOrEmpty(str) ? str : str.Substring(0, Math.Min(str.Length, maxLength));
    }
}