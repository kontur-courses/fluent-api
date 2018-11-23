using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private ImmutableList<Type> excludedTypes = ImmutableList<Type>.Empty;
        private ImmutableList<String> excludedProperties = ImmutableList<String>.Empty;
        private ImmutableDictionary<Type, CultureInfo> typeCultures = ImmutableDictionary<Type, CultureInfo>.Empty;
        private ImmutableDictionary<Type, Delegate> typeSerializations = ImmutableDictionary<Type, Delegate>.Empty;
        private ImmutableDictionary<String, Delegate> propertySerializations = ImmutableDictionary<String, Delegate>.Empty;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var propertyName = propertyInfo.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var currentConfig = (PrintingConfig<TOwner>) MemberwiseClone();
            var propertyInfo = (PropertyInfo)((MemberExpression) memberSelector.Body).Member;
            var propertyName = propertyInfo.Name;
            currentConfig.excludedProperties = excludedProperties.Add(propertyName);
            return currentConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var currentConfig = (PrintingConfig<TOwner>) MemberwiseClone();
            excludedTypes = excludedTypes.Add(typeof(TPropType));
            return currentConfig;
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

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {

                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if (excludedProperties.Contains(propertyInfo.Name)) continue;
                var propetyValue = String.Empty;
                if (typeSerializations.ContainsKey(propertyInfo.PropertyType))
                    propetyValue = typeSerializations[propertyInfo.PropertyType]
                               .DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
                if (propertySerializations.ContainsKey(propertyInfo.Name))
                    propetyValue = propertySerializations[propertyInfo.Name]
                               .DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
                if (typeCultures.ContainsKey(propertyInfo.PropertyType))
                    propetyValue = ((IFormattable)propertyInfo.GetValue(obj))
                           .ToString(null, typeCultures[propertyInfo.PropertyType]) + Environment.NewLine;
                if (propetyValue == String.Empty)
                    propetyValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                sb.Append(identation + propertyInfo.Name + " = " + propetyValue);
            }
            return sb.ToString();
        }

        internal void ChangeCultureForType(Type type, CultureInfo cultureInfo)
        {
            typeCultures = typeCultures.SetItem(type, cultureInfo);
        }

        internal void ChangeSerializationForType(Type type, Delegate serializator)
        {
            typeSerializations = typeSerializations.SetItem(type, serializator);
        }

        internal void ChangeSerializationForProperty(string propertyName, Delegate serializator)
        {
            propertySerializations = propertySerializations.SetItem(propertyName, serializator);
        }
    }
}