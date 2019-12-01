using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, specialPrintingFunctionsForTypes);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(
                this, specialPrintingFunctionsForProperties, ExtractPropertyInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(ExtractPropertyInfo(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private static PropertyInfo ExtractPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                return (PropertyInfo) memberExpression.Member;
            throw new ArgumentException("Member selector should be member expression");
        }

        private string PrintToString(object obj, int nestingLevel, bool newLineRequested = true)
        {
            if (obj == null)
                return "null" + (newLineRequested ? Environment.NewLine : "");
            objectsLevels[obj] = nestingLevel;
            return TryToPrintType(obj, nestingLevel, newLineRequested) ?? PrintProperties(obj, nestingLevel);
        }

        private string TryToPrintType(object obj, int nestingLevel, bool newLineRequested = true)
        {
            var type = obj.GetType();
            if (excludedTypes.Contains(type))
                return "";
            if (specialPrintingFunctionsForTypes.ContainsKey(type))
                return specialPrintingFunctionsForTypes[type](obj) + Environment.NewLine;
            if (finalTypes.Contains(type))
                return obj + (newLineRequested ? Environment.NewLine : "");
            if (typeof(IDictionary).IsAssignableFrom(type))
                return PrintDictionary(obj, nestingLevel);
            if (typeof(IEnumerable).IsAssignableFrom(type))
                return PrintEnumerable(obj, nestingLevel);
            return null;
        }

        private string PrintEnumerable(object obj, int nestingLevel)
        {
            var enumerable = ((IEnumerable)obj).Cast<object>();
            return string.Join("; ", enumerable
                .Select(n => PrintToString(n, nestingLevel + 1, false)))
                   + Environment.NewLine;
        }

        private string PrintDictionary(object obj, int nestingLevel)
        {
            var dictionary = ((IDictionary)obj);
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                var printedKeyType = TryToPrintType(key, 0, false);
                var printedValue = PrintToString(value, nestingLevel + 2, false);
                var indentation = new string('\t', nestingLevel + 1);
                sb.Append(indentation + (printedKeyType ?? key.GetType().ToString())
                                      + " = " + printedValue + Environment.NewLine);
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var name = propertyInfo.Name;
                var value = propertyInfo.GetValue(obj);
                if (excludedProperties.Contains(propertyInfo)
                    || value != null && excludedTypes.Contains(value.GetType()))
                {
                    continue;
                }
                if (value != null && objectsLevels.ContainsKey(value) && objectsLevels[value] < nestingLevel)
                {
                    sb.Append(indentation + name + " contains cyclic reference" + Environment.NewLine);
                }
                else if (specialPrintingFunctionsForProperties.ContainsKey(propertyInfo))
                {
                    sb.Append(indentation +
                              specialPrintingFunctionsForProperties[propertyInfo](value) + Environment.NewLine);
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