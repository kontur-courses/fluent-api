using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Dictionary<PropertyInfo, Delegate> propPrintingMethods =
            new Dictionary<PropertyInfo, Delegate>();

        private readonly HashSet<PropertyInfo> propsToExclude = new HashSet<PropertyInfo>();
        private readonly Dictionary<Type, CultureInfo> typePrintingCultureInfo = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typePrintingMethods = new Dictionary<Type, Delegate>();
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();

        private PropertyInfo propertyToConfig;

        public PrintingConfig<TOwner> AddPrintingMethod<TPropType>(Func<TPropType, string> method)
        {
            if (propertyToConfig == null)
            {
                typePrintingMethods[typeof(TPropType)] = method;
            }
            else
            {
                propPrintingMethods[propertyToConfig] = method;
                propertyToConfig = null;
            }

            return this;
        }

        public PrintingConfig<TOwner> AddPrintingCulture<TPropType>(CultureInfo cultureInfo)
        {
            typePrintingCultureInfo[typeof(TPropType)] = cultureInfo;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            propertyToConfig = (PropertyInfo) body.Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression body)
            {
                propsToExclude.Add((PropertyInfo) body.Member);
            }

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            if (finalTypes.Contains(obj.GetType()))
            {
                return obj + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            if (obj is IEnumerable collection)
            {
                return PrintCollectionToString(nestingLevel,
                    sb,
                    collection,
                    indentation);
            }

            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var props = type.GetProperties().Where(prop =>
                !typesToExclude.Contains(prop.PropertyType) && !propsToExclude.Contains(prop));
            foreach (var propertyInfo in props)
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo, obj, nestingLevel));
            }

            return sb.ToString();
        }

        private string PrintCollectionToString(int nestingLevel, StringBuilder sb, IEnumerable collection,
            string indentation)
        {
            sb.AppendLine();
            foreach (var elem in collection)
            {
                sb.Append(indentation + PrintToString(elem, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string PrintToString(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var value = propertyInfo.GetValue(obj) as dynamic;
            var type = propertyInfo.PropertyType;

            if (propPrintingMethods.TryGetValue(propertyInfo, out var printProperty))
            {
                return printProperty.DynamicInvoke(value) + Environment.NewLine;
            }

            if (typePrintingMethods.TryGetValue(type, out var printType))
            {
                return printType.DynamicInvoke(value) + Environment.NewLine;
            }

            if (typePrintingCultureInfo.TryGetValue(type, out var cultureInfo))
            {
                return value?.ToString(cultureInfo) + Environment.NewLine;
            }

            return PrintToString(value, nestingLevel + 1);
        }
    }
}