using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly int nestingLimit = 15;
        private readonly string newLine = Environment.NewLine;
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly Type[] finalTypes =
        {
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private readonly Dictionary<Type, CultureInfo> numberCultures = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<PropertyInfo, Func<object, string>> propertyPrintingMethods =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> typePrintingMethods =
            new Dictionary<Type, Func<object, string>>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var property = (PropertyInfo) ((MemberExpression) memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, property);
        }

        private void SetTypePrintingFormat<TPropType>(Func<object, string> printing)
        {
            var type = typeof(TPropType);
            if (typePrintingMethods.ContainsKey(type))
                typePrintingMethods.Add(type, printing);
            else
                typePrintingMethods[type] = printing;
        }

        private void SetPropertyPrintingFormat(PropertyInfo property, Func<object, string> printing)
        {
            if (propertyPrintingMethods.ContainsKey(property))
                propertyPrintingMethods.Add(property, printing);
            else
                propertyPrintingMethods[property] = printing;
        }

        private void SetCulture<TPropType>(CultureInfo culture)
        {
            var type = typeof(TPropType);
            if (numberCultures.ContainsKey(type))
                numberCultures.Add(type, culture);
            else
                numberCultures[type] = culture;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add((PropertyInfo) ((MemberExpression) memberSelector.Body).Member);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj,"");
        }

        private string PrintToString(object obj, string indentation)
        {
            if (indentation.Length >= nestingLimit)
                return "Warning! Nesting limit was reached!"; 
            
            if (obj == null)
                return $"null{newLine}";

            var type = obj.GetType();

            if (finalTypes.Contains(type))
                return $"{obj}{newLine}";
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{indentation}{type.Name}");
            
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);
                var propertyType = propertyInfo.PropertyType;
                if (IsExcluded(propertyInfo, propertyType))
                    continue;
                stringBuilder.Append(ConvertPropertyValueToString(
                    $"{indentation}\t",
                    propertyType,
                    propertyInfo, 
                    propertyValue));
            }

            return stringBuilder.ToString();
        }

        private string ConvertPropertyValueToString(string indentation, Type propertyType, PropertyInfo propertyInfo,
            object propertyValue)
        {
            var stringBuilder = new StringBuilder();
            string stringData;
            if (numberCultures.ContainsKey(propertyType))
                stringData = string.Format(numberCultures[propertyType], "{0}", propertyValue);
            else if (typePrintingMethods.ContainsKey(propertyType))
            {
                var typePrintingMethod = typePrintingMethods[propertyType];
                stringData= $"{typePrintingMethod.DynamicInvoke(propertyValue)}{newLine}";
            }
            else if (propertyPrintingMethods.ContainsKey(propertyInfo))
            {
                var propertyPrintingMethod = propertyPrintingMethods[propertyInfo];
                stringData= $"{propertyPrintingMethod.DynamicInvoke(propertyValue)}{newLine}";
            }
            else if (propertyValue is IEnumerable collection && !(propertyValue is string))
                stringData = CollectionToString($"{indentation}\t", collection);
            else
                stringData = PrintToString(propertyValue, $"{indentation}\t");

            stringBuilder.Append($"{indentation}{propertyInfo.Name} = {stringData}");
            return stringBuilder.ToString();
        }
        
        private string CollectionToString(string indentation, IEnumerable collection)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{collection.GetType().Name}{newLine}");
            stringBuilder.Append($"{indentation}[{newLine}");
            foreach (var element in collection)
                stringBuilder.Append($"{PrintToString(element, $"{indentation}\t")}");
            stringBuilder.Append($"{indentation}]{newLine}");
            return stringBuilder.ToString();
        }

        private bool IsExcluded(PropertyInfo propertyInfo, Type propertyType)
        {
            return excludedTypes.Contains(propertyType) || excludedProperties.Contains(propertyInfo);
        }

        public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
        {
            private readonly PrintingConfig<TOwner> printingConfig;
            private readonly PropertyInfo property;

            public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property = null)
            {
                this.printingConfig = printingConfig;
                this.property = property;
            }

            PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => printingConfig;

            void IPropertyPrintingConfig<TOwner>.SetCulture<TType>(CultureInfo cultureInfo)
            {
                printingConfig.SetCulture<TType>(cultureInfo);
            }

            public PrintingConfig<TOwner> Using(Func<TPropType, string> printing)
            {
                string PrintingObject(object o) => printing((TPropType) o);

                if (property != null)
                    printingConfig.SetPropertyPrintingFormat(property, PrintingObject);
                else
                    printingConfig.SetTypePrintingFormat<TPropType>(PrintingObject);
                return printingConfig;
            }
        }

        public interface IPropertyPrintingConfig<TOwner>
        {
            PrintingConfig<TOwner> ParentConfig { get; }
            void SetCulture<TType>(CultureInfo cultureInfo) where TType : IFormattable;
        }
    }
}