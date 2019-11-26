using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<Type> excludingTypes;
        private readonly Dictionary<Type, Func<object, string>> customTypesPrints;

        Dictionary<Type, Func<object, string>> IPrintingConfig.CustomTypesPrints => customTypesPrints;

        private readonly List<string> excludingProperties;
        private readonly Dictionary<string, Func<object, string>> customPropertiesPrints;

        Dictionary<string, Func<object, string>> IPrintingConfig.CustomPropertiesPrints => customPropertiesPrints;

        private int maxNumberListItems;

        private readonly List<object> referenceObjects;

        public PrintingConfig()
        {
            excludingTypes = new List<Type>();
            customTypesPrints = new Dictionary<Type, Func<object, string>>();

            excludingProperties = new List<string>();
            customPropertiesPrints = new Dictionary<string, Func<object, string>>();

            maxNumberListItems = -1;

            referenceObjects = new List<object>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        protected string PrintToString(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);

            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(decimal), typeof(Guid)
            };
            var type = obj.GetType();
            if (type.IsPrimitive || finalTypes.Contains(type))
                return obj + Environment.NewLine;
            if (obj is IEnumerable enumerable)
            {
                return PrintToStringForIEnumerable(enumerable, nestingLevel + 1);
            }

            return PrintToStringForProperty(obj, nestingLevel + 1);
        }

        private string PrintToStringForProperty(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);

            if (referenceObjects.Contains(obj))
                return string.Format(
                    "Is higher in the hierarchy by {0} steps" + Environment.NewLine,
                    referenceObjects.Count - referenceObjects.IndexOf(obj) - 1);

            referenceObjects.Add(obj);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType) ||
                    excludingProperties.Contains(propertyInfo.Name))
                    continue;
                var str = "";
                if (customTypesPrints.ContainsKey(propertyInfo.PropertyType))
                {
                    str = customTypesPrints[propertyInfo.PropertyType](propertyInfo.GetValue(obj)) + Environment.NewLine;
                }
                else if (customPropertiesPrints.ContainsKey(propertyInfo.Name))
                {
                    str = customPropertiesPrints[propertyInfo.Name](propertyInfo.GetValue(obj)) + Environment.NewLine;
                }
                else
                {
                    str = PrintToString(propertyInfo.GetValue(obj), nestingLevel);
                }
                sb.Append(identation + propertyInfo.Name + " = " + str);
            }
            referenceObjects.RemoveAt(referenceObjects.Count - 1);
            return sb.ToString();
        }

        private string PrintToStringForIEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = enumerable.GetType();
            sb.AppendLine(type.Name);
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

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ChangePrintFor<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this, ((MemberExpression)func.Body).Member as PropertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            excludingProperties.Add((((MemberExpression)func.Body).Member as PropertyInfo).Name);
            return this;
        }

        public PrintingConfig<TOwner> SetMaxNumberListItems(int maxNumberListItems)
        {
            this.maxNumberListItems = maxNumberListItems;
            return this;
        }
    }
}