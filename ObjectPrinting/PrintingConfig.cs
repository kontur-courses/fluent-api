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
        private readonly HashSet<Type> excludedPropertiesTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedPropertiesNames = new HashSet<string>();

        private readonly Dictionary<Type, IPropertyPrintingConfig<TOwner>> propertiesConfigByType =
            new Dictionary<Type, IPropertyPrintingConfig<TOwner>>();

        private readonly Dictionary<string, IPropertyPrintingConfig<TOwner>> propertiesConfigByName =
            new Dictionary<string, IPropertyPrintingConfig<TOwner>>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            propertiesConfigByType[typeof(TPropType)] = propertyConfig;
            return propertyConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            propertiesConfigByName[propertyName] = propertyConfig;
            return propertyConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            excludedPropertiesNames.Add(propertyName);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedPropertiesTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (nestingLevel > 1000)
                return "[Infinite Recursion]";

            if (TryPrintFinalType(obj, out var result))
                return result;

            if (TryPrintCollection(obj, out result))
                return result;

            return PrintAnyType(obj, nestingLevel);
        }

        private string PrintAnyType(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var type = obj.GetType();
            var includedProperties = GetIncludedProperties(type);
            builder.AppendLine(includedProperties.Any() ? type.Name : obj.ToString());
            foreach (var propertyInfo in includedProperties)
            {
                builder.Append(indentation + propertyInfo.Name + " = " +
                               PrintProperty(obj, nestingLevel, propertyInfo));
            }
            return builder.ToString();
        }

        private PropertyInfo[] GetIncludedProperties(Type type)
        {
            return type.GetProperties()
                .Where(property => !excludedPropertiesTypes.Contains(property.PropertyType))
                .Where(property => !excludedPropertiesNames.Contains(property.Name))
                .ToArray();
        }

        private bool TryPrintCollection(object obj, out string result)
        {
            var builder = new StringBuilder();
            result = null;
            if (!(obj is IEnumerable collection))
                return false;
            builder.AppendLine(obj.GetType().Name + Environment.NewLine + "{");
            AddCollectionItems(collection, builder);
            builder.Append("}" + Environment.NewLine);
            result = builder.ToString();
            return true;
        }

        private void AddCollectionItems(IEnumerable collection, StringBuilder builder)
        {
            var number = 0;
            foreach (var item in collection)
            {
                if (number > 1000)
                {
                    builder.Append("\t..." + Environment.NewLine);
                    break;
                }
                builder.Append("\t" + PrintToString(item, 1));
                number++;
            }
        }

        private string PrintProperty(object obj, int nestingLevel, PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            Func<object, string> printingFunction = property => PrintToString(property, nestingLevel + 1);
            if (propertiesConfigByType.TryGetValue(propertyType, out var config) && config.PrintingFunction != null)
                printingFunction = GetPrintingFunctionWithNewLine(config.PrintingFunction);
            if (propertiesConfigByName.TryGetValue(propertyInfo.Name, out config) && config.PrintingFunction != null)
                printingFunction = GetPrintingFunctionWithNewLine(config.PrintingFunction);
            return printingFunction(propertyInfo.GetValue(obj));
        }

        private static Func<object, string> GetPrintingFunctionWithNewLine(Func<object, string> printingFunction)
        {
            return obj => printingFunction(obj) + Environment.NewLine;
        }

        private bool TryPrintFinalType(object obj, out string result)
        {
            result = null;
            var type = obj.GetType();

            var numberTypes = new[] { typeof(int), typeof(double), typeof(float), typeof(long) };
            var finalTypes = new[] { typeof(string), typeof(DateTime), typeof(TimeSpan) }.Concat(numberTypes);

            if (!finalTypes.Contains(type))
                return false;

            var objectString = obj.ToString();
            if (numberTypes.Contains(type) && propertiesConfigByType.TryGetValue(type, out var config))
                objectString = ((IConvertible)obj).ToString(config.Culture);

            result = objectString + Environment.NewLine;
            return true;
        }
    }
}