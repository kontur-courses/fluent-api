using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Expression<Func<object, string>>> PrintersForTypes { get; }
        Dictionary<Type, CultureInfo> CultureInfoForTypes { get; }
        Dictionary<string, Expression<Func<object, string>>> PrintersForPropertiesNames { get; }
        int? MaxLength { get; set; }
    }

    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<string> excluded = new List<string>();

        private readonly Dictionary<Type, CultureInfo> cultureInfoForTypes =
            new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Expression<Func<object, string>>> printersForTypes =
            new Dictionary<Type, Expression<Func<object, string>>>();
        private readonly Dictionary<string, Expression<Func<object, string>>> printersForPropertiesName =
            new Dictionary<string, Expression<Func<object, string>>>();
        private int? maxLength;

        Dictionary<Type, Expression<Func<object, string>>> IPrintingConfig.PrintersForTypes => printersForTypes;
        Dictionary<Type, CultureInfo> IPrintingConfig.CultureInfoForTypes => cultureInfoForTypes;
        Dictionary<string, Expression<Func<object, string>>> IPrintingConfig.PrintersForPropertiesNames => printersForPropertiesName;
        int? IPrintingConfig.MaxLength { get => maxLength; set => maxLength = value; }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excluded.Add(((MemberExpression)memberSelector.Body).Member.Name);
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

        private string GetPropertyPrintingValue(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType))
                return string.Empty;

            if (excluded.Contains(propertyInfo.Name))
                return string.Empty;

            if (printersForTypes.ContainsKey(propertyInfo.PropertyType))
                return propertyInfo.Name + " = " + printersForTypes[propertyInfo.PropertyType].Compile().Invoke(propertyInfo.GetValue(obj));

            if (cultureInfoForTypes.ContainsKey(propertyInfo.PropertyType))
            {
                var type = propertyInfo.PropertyType;
                if (type == typeof(int))
                    return propertyInfo.Name + " = " + ((int)propertyInfo.GetValue(obj)).ToString(cultureInfoForTypes[propertyInfo.PropertyType]);
                if (type == typeof(double))
                    return propertyInfo.Name + " = " + ((double)propertyInfo.GetValue(obj)).ToString(cultureInfoForTypes[propertyInfo.PropertyType]);
                if (type == typeof(long))
                    return propertyInfo.Name + " = " + ((long)propertyInfo.GetValue(obj)).ToString(cultureInfoForTypes[propertyInfo.PropertyType]);

            }

            if (printersForPropertiesName.ContainsKey(propertyInfo.Name))
                return propertyInfo.Name + " = " + printersForPropertiesName[propertyInfo.Name].Compile().Invoke(propertyInfo.GetValue(obj));

            if (propertyInfo.PropertyType == typeof(string) &&
                maxLength != null &&
                PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1).Length > maxLength)
            {
                return propertyInfo.Name + " = " +
                       PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1).Substring(0, (int)maxLength);
            }

            return propertyInfo.Name + " = " +
                   PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
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
            if (type.Implements(typeof(ICollection)))
            {
                sb.Append(GetICollectionPrintingValue((ICollection)obj, nestingLevel));
            }
            else
            {

                foreach (var propertyInfo in type.GetProperties())
                {
                    sb.Append(identation + GetPropertyPrintingValue(propertyInfo, obj, nestingLevel));
                }
            }

            return sb.ToString();
        }



        private string GetICollectionPrintingValue(ICollection collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var index = 0;

            foreach (var obj in collection)
            {
                sb.Append(identation);
                sb.Append($"{index}: {PrintToString(obj, nestingLevel)}");
                index++;
            }

            return sb.ToString();
        }
    }
}