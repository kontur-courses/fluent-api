using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly PrintingConfigData printingConfigData = new PrintingConfigData();


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            printingConfigData.ExcludingPropertiesNames.Add(memberInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            printingConfigData.ExcludingTypes.Add(typeof(TPropType));
            return this;
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
            {
                if (obj is IFormattable formattable &&
                    printingConfigData.NumberTypesToCulture.TryGetValue(formattable.GetType(),
                        out var cultureInfo))
                {
                    return formattable.ToString("", cultureInfo) + Environment.NewLine;
                }

                return obj + Environment.NewLine;
            }


            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (obj is IEnumerable collection)
            {
                sb.AppendLine(indentation + GetIenumerableContent(collection));
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.IsIndexer())
                    continue;
                var propertyValue = propertyInfo.GetValue(obj);
                if (printingConfigData.VisitedProperties.Contains(propertyValue))
                {
                    sb.AppendLine(indentation + propertyInfo.Name + ": Cyclic...");
                    continue;
                }

                if (!finalTypes.Contains(propertyInfo.PropertyType) && propertyValue != null)
                    printingConfigData.VisitedProperties.Add(propertyValue);

                if (printingConfigData.ExcludingTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (printingConfigData.ExcludingPropertiesNames.Contains(propertyInfo))
                    continue;

                var content = GetContent(propertyInfo, obj, nestingLevel);
                var header = indentation + propertyInfo.Name + " = ";
                sb.Append(header + content);
            }

            return sb.ToString();
        }

        private string GetIenumerableContent(IEnumerable obj)
        {
            var contentList = new List<object>();
            foreach (var e in obj)
            {
                var content = "";
                if (e is IFormattable formattable &&
                    printingConfigData.NumberTypesToCulture.TryGetValue(e.GetType(), out var cultureInfo))
                    content = formattable.ToString("", cultureInfo);
                else
                {
                    content = e.ToString();
                }

                contentList.Add(content);
            }

            return string.Join(", ", contentList);
        }


        private string GetContent(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var propertyValue = propertyInfo.GetValue(obj);
            if (printingConfigData.PropNamesPrintingFunctions.TryGetValue(propertyInfo, out var val))
            {
                return val.DynamicInvoke(propertyValue).ToString();
            }

            if (printingConfigData.TypePrintingFunctions.TryGetValue(propertyInfo.PropertyType, out val))
            {
                return val.DynamicInvoke(propertyValue).ToString();
            }

            if (printingConfigData.StringPropNamesTrimming.TryGetValue(propertyInfo, out var length))
            {
                var maxLength = propertyValue.ToString().Length;
                return PrintToString(propertyValue, nestingLevel + 1)
                    .Substring(0, Math.Min(maxLength, length));
            }


            return PrintToString(propertyValue, nestingLevel + 1);
        }

        PrintingConfigData IPrintingConfig<TOwner>.PrintingConfigData => printingConfigData;
    }

    public interface IPrintingConfig<TOwner>
    {
        PrintingConfigData PrintingConfigData { get; }
    }
}