using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Core.PropertyPrintingConfig;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Core.PrintingConfig
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();

        private readonly Dictionary<Type, Func<object, string>> typePrintingFunctions =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<object, string>> propertyPrintingFunctions =
            new Dictionary<string, Func<object, string>>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();
        private readonly HashSet<object> processedObjects = new HashSet<object>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(long),
            typeof(short), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
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

            processedObjects.Add(objectToPrint);

            var builder = new StringBuilder();
            var indentation = new string('\t', nestingLevel);
            AppendType(objectType, builder, indentation);
            var childIndentation = new string('\t', nestingLevel + 1);

            foreach (var itemInfo in objectToPrint.GetItems())
            {
                if (processedObjects.Contains(itemInfo.Item))
                {
                    builder.AppendLine(childIndentation + $"Cyclic reference {itemInfo.Name}");
                    continue;
                }

                if (itemInfo.Name == null)
                {
                    builder.Append(PrintToString(itemInfo.Item, nestingLevel + 1));
                }
                else
                {
                    AppendItem(itemInfo, builder, nestingLevel, childIndentation);
                }
            }

            return builder.ToString();
        }

        private static void AppendType(Type objectType, StringBuilder builder, string indentation)
        {
            if (objectType.IsGenericType)
            {
                builder.Append(objectType.Name.Split('`')[0]);
                var genericArgumentTypes = objectType.GetGenericArguments().Select(type => type.Name);
                builder.Append($"<{string.Join(", ", genericArgumentTypes)}>");
            }
            else
            {
                if (!objectType.IsArray)
                {
                    builder.Append(indentation);
                }

                builder.Append(objectType.Name);
            }

            builder.Append("\n");
        }

        private void AppendItem(ItemInfo itemInfo, StringBuilder builder, int nestingLevel, string indentation)
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

        Dictionary<Type, Func<object, string>> IPrintingConfig.TypePrintingFunctions => typePrintingFunctions;

        Dictionary<string, Func<object, string>> IPrintingConfig.PropertyPrintingFunctions =>
            propertyPrintingFunctions;

        Dictionary<Type, CultureInfo> IPrintingConfig.TypeCultures => typeCultures;
    }
}