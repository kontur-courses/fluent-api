using System;
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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        private readonly Dictionary<Type, Func<object, string>> typePrintingMethods =
            new Dictionary<Type, Func<object, string>>();
        
        private readonly Dictionary<PropertyInfo, Func<object, string>> propertyPrintingMethods =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly Dictionary<Type, CultureInfo> numberCultures = new Dictionary<Type, CultureInfo>();

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var property = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
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
        
        private void SetPropertyPrintingFormat(PropertyInfo property,Func<object, string> printing)
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
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
           
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);
                var propertyType = propertyInfo.PropertyType;

                if (IsExcluded(propertyInfo, propertyType))
                    continue;
                
                var print = PropertyToString(nestingLevel, propertyType, propertyInfo, propertyValue);
                sb.Append($"{indentation}{propertyInfo.Name} = {print}");
            }

            return sb.ToString();
        }

        private string PropertyToString(int nestingLevel, Type propertyType, PropertyInfo propertyInfo, object propertyValue)
        {
            string print;
            if (typePrintingMethods.ContainsKey(propertyType))
            {
                var typePrintingMethod = typePrintingMethods[propertyType];
                print = $"{typePrintingMethod.Invoke(propertyValue)}{Environment.NewLine}";
            }
            else if (propertyPrintingMethods.ContainsKey(propertyInfo))
            {
                var propertyPrintingMethod = propertyPrintingMethods[propertyInfo];
                print = $"{propertyPrintingMethod.Invoke(propertyValue)}{Environment.NewLine}";
            }
            else
                print = PrintToString(propertyValue, nestingLevel + 1);

            return print;
        }

        private bool IsExcluded(PropertyInfo propertyInfo, Type propertyType)
        {
            return excludedTypes.Contains(propertyType) || excludedProperties.Contains(propertyInfo);
        }

        public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
        {
            private readonly PrintingConfig<TOwner> printingConfig;
            private readonly PropertyInfo property;

            public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,  PropertyInfo property = null)
            {
                this.printingConfig = printingConfig;
                this.property = property;
            }

            public PrintingConfig<TOwner> Using(Func<TPropType, string> prining)
            {
                string printingObject(object o) => prining((TPropType) o);
                
                if (property != null)
                    printingConfig.SetPropertyPrintingFormat(property, printingObject);
                else
                    printingConfig.SetTypePrintingFormat<TPropType>(printingObject);
                return printingConfig;
            }

            public PrintingConfig<TOwner> Using(CultureInfo culture)
            {
                printingConfig.SetCulture<TPropType>(culture);
                return printingConfig;
            }

            PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        }

        public interface IPropertyPrintingConfig<TOwner, TPropType>
        {
            PrintingConfig<TOwner> ParentConfig { get; }
        }
    }
}