using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> typePrintingFunctions = new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, Delegate> propertyPrintingFunctions =
            new Dictionary<string, Delegate>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = PrintingConfigUtils.GetPropertyNameFromMemberSelector(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = PrintingConfigUtils.GetPropertyNameFromMemberSelector(memberSelector);
            if (propertyName != null)
            {
                excludedProperties.Add(propertyName);
            }

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

        private string PrintToString(object objectToPrint, int nestingLevel)
        {
            if (objectToPrint == null)
            {
                return "null" + Environment.NewLine;
            }

            var objectType = objectToPrint.GetType();

            if (excludedTypes.Contains(objectType))
            {
                return string.Empty;
            }

            if (finalTypes.Contains(objectType))
            {
                if (typeCultures.TryGetValue(objectType, out var cultureInfo))
                {
                    return Convert.ToString(objectToPrint, cultureInfo) + Environment.NewLine;
                }

                return objectToPrint + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();

            builder.AppendLine(objectType.Name);
            foreach (var itemInfo in objectToPrint.GetItems())
            {
                AppendItem(nestingLevel, itemInfo, builder, indentation);
            }

            return builder.ToString();
        }

        private void AppendItem(int nestingLevel, ItemInfo itemInfo, StringBuilder builder, string indentation)
        {
            if (excludedTypes.Contains(itemInfo.Type) || excludedProperties.Contains(itemInfo.Name))
            {
                return;
            }

            builder.Append(indentation + itemInfo.Name + " = ");
            if (propertyPrintingFunctions.TryGetValue(itemInfo.Name, out var propertyPrintingFunction))
            {
                builder.Append(PrintToString(
                    propertyPrintingFunction.DynamicInvoke(itemInfo.Item), nestingLevel + 1));
            }
            else if (typePrintingFunctions.TryGetValue(itemInfo.Type, out var typePrintingFunction))
            {
                builder.Append(PrintToString(
                    typePrintingFunction.DynamicInvoke(itemInfo.Item), nestingLevel + 1));
            }
            else
            {
                builder.Append(PrintToString(itemInfo.Item, nestingLevel + 1));
            }
        }

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypePrintingFunctions => typePrintingFunctions;

        Dictionary<string, Delegate> IPrintingConfig<TOwner>.PropertyPrintingFunctions =>
            propertyPrintingFunctions;

        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.TypeCultures => typeCultures;
    }
}