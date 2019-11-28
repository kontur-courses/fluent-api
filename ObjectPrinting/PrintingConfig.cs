using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes =
            new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<PropertyInfo, Func<object, string>> specialPrintingFunctionsForProperties =
            new Dictionary<PropertyInfo, Func<object, string>>();
        private readonly Dictionary<object, int> objectsLevels = new Dictionary<object, int>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, specialPrintingFunctionsForTypes);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(
                this, specialPrintingFunctionsForProperties, memberSelector.GetPropertyInfo());
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(memberSelector.GetPropertyInfo());
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            objectsLevels[obj] = nestingLevel;
            return TryToPrintType(obj) ?? PrintProperties(obj, nestingLevel);
        }

        private string TryToPrintType(object obj)
        {
            var type = obj.GetType();
            if (excludedTypes.Contains(type))
                return "";
            if (specialPrintingFunctionsForTypes.ContainsKey(type))
                return specialPrintingFunctionsForTypes[type](obj);
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            return null;
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo))
                    continue;
                var name = propertyInfo.Name;
                var value = propertyInfo.GetValue(obj);
                if (objectsLevels.ContainsKey(value) && objectsLevels[value] < nestingLevel)
                    sb.Append(indentation + name + " contains cyclic reference");
                else if (specialPrintingFunctionsForProperties.ContainsKey(propertyInfo))
                {
                    sb.Append(indentation +
                              specialPrintingFunctionsForProperties[propertyInfo](value));
                }
                else
                {
                    sb.Append(indentation + name + " = " +
                              PrintToString(value, nestingLevel + 1));
                }
            }
            return sb.ToString();
        }
    }
}