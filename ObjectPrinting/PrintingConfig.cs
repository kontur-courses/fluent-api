using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<Type> excludingTypes;
        private readonly Dictionary<Type, Func<object, string>> customTypesPrints;

        Dictionary<Type, Func<object, string>> IPrintingConfig.CustomTypesPrints => customTypesPrints;

        private readonly List<PropertyInfo> excludingProperties;
        private readonly Dictionary<PropertyInfo, Func<object, string>> customPropertiesPrints;

        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig.CustomPropertiesPrints => customPropertiesPrints;

        private int maxNumberListItems;

        private readonly List<object> referenceObjects;

        private Dictionary<Type, CultureInfo> cultureInfo;

        Dictionary<Type, CultureInfo> IPrintingConfig.CultureInfo => cultureInfo;

        public PrintingConfig()
        {
            excludingTypes = new List<Type>();
            customTypesPrints = new Dictionary<Type, Func<object, string>>();

            excludingProperties = new List<PropertyInfo>();
            customPropertiesPrints = new Dictionary<PropertyInfo, Func<object, string>>();

            maxNumberListItems = -1;

            referenceObjects = new List<object>();

            cultureInfo = new Dictionary<Type, CultureInfo>();
        }

        public PrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            excludingTypes = new List<Type>(printingConfig.excludingTypes);
            customTypesPrints = new Dictionary<Type, Func<object, string>>(printingConfig.customTypesPrints);

            excludingProperties =new List<PropertyInfo>(printingConfig.excludingProperties);
            customPropertiesPrints = new Dictionary<PropertyInfo, Func<object, string>>(printingConfig.customPropertiesPrints);

            maxNumberListItems = printingConfig.maxNumberListItems;

            referenceObjects = new List<object>(printingConfig.referenceObjects);

            cultureInfo = new Dictionary<Type, CultureInfo>(printingConfig.cultureInfo);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return new PrintingConfig<TOwner>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(new PrintingConfig<TOwner>(this));
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(
                new PrintingConfig<TOwner>(this),
                ((MemberExpression)func.Body).Member as PropertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var propertyFullName = func.Body.ToString();
            propertyFullName = propertyFullName.Substring(propertyFullName.IndexOf('.'));
            excludingProperties.Add(((MemberExpression)func.Body).Member as PropertyInfo);
            return new PrintingConfig<TOwner>(this);
        }

        public PrintingConfig<TOwner> SetMaxNumberListItems(int maxNumberListItems)
        {
            this.maxNumberListItems = maxNumberListItems;
            return new PrintingConfig<TOwner>(this);
        }

        private bool FinalÑheck(Type type)
        {
            var finalTypes = new[]
            {
                typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(decimal), typeof(Guid)
            };
            return type.IsPrimitive || finalTypes.Contains(type);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (FinalÑheck(type))
            {
                if (!cultureInfo.ContainsKey(type))
                    return obj + Environment.NewLine;
                var culture = cultureInfo[type];
                dynamic number = Convert.ChangeType(obj, type);
                return number.ToString(culture) + Environment.NewLine;

            }

            if (referenceObjects.Contains(obj))
                return string.Format(
                    "Is higher in the hierarchy by {0} steps" + Environment.NewLine,
                    referenceObjects.Count - referenceObjects.IndexOf(obj) - 1);

            referenceObjects.Add(obj);

            string str;
            if (obj is IEnumerable enumerable)
                str = PrintToStringForIEnumerable(enumerable, nestingLevel + 1);
            else
                str = PrintToStringForProperty(obj, nestingLevel + 1);

            referenceObjects.RemoveAt(referenceObjects.Count - 1);

            return str;
        }

        private string PrintToStringForProperty(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType) ||
                    excludingProperties.Contains(propertyInfo))
                    continue;
                var str = "";
                if (customTypesPrints.ContainsKey(propertyInfo.PropertyType))
                {
                    str = customTypesPrints[propertyInfo.PropertyType](propertyInfo.GetValue(obj)) + Environment.NewLine;
                }
                else if (customPropertiesPrints.ContainsKey(propertyInfo))
                {
                    str = customPropertiesPrints[propertyInfo](propertyInfo.GetValue(obj)) + Environment.NewLine;
                }
                else
                {
                    str = PrintToString(propertyInfo.GetValue(obj), nestingLevel);
                }
                sb.Append(identation + propertyInfo.Name + " = " + str);
            }
            return sb.ToString();
        }

        private string PrintToStringForIEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = enumerable.GetType();
            sb.AppendLine(type.Name);
            var identation = new string('\t', nestingLevel);
            if (excludingTypes.Contains(type))
                return "";
            if (customTypesPrints.ContainsKey(type))
            {
                sb.Append(identation + customTypesPrints[type](enumerable) + Environment.NewLine);
                return sb.ToString();
            }

            var count = 0;
            foreach (var item in enumerable)
            {
                sb.Append(identation + PrintToString(item, nestingLevel));
                count++;
                if (count >= maxNumberListItems && maxNumberListItems >= 0)
                    break;
            }
            return sb.ToString();
        }
    }
}