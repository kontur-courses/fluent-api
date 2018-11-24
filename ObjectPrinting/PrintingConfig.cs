using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();
        private readonly HashSet<string> propertiesToExclude = new HashSet<string>();

        private readonly Dictionary<Type, Func<object, string>> printersForTypes =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<object, string>> printersForProperties =
            new Dictionary<string, Func<object, string>>();


        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyInfo = ExtractPropertyInfo(propertySelector);
            propertiesToExclude.Add(propertyInfo.Name);
            return this;
        }


        private PropertyInfo ExtractPropertyInfo<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Delegate is not a member selector");
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Delegate selects not a property");
            return propertyInfo;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            printersForTypes[typeof(TPropType)] =
                obj => (printingConfig as IPropertyPrintingConfig<TOwner, TPropType>).PrintingFunction((TPropType) obj);
            return printingConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            var propertyInfo = ExtractPropertyInfo(propertySelector);
            printersForProperties[propertyInfo.Name] = 
                obj => ((IPropertyPrintingConfig<TOwner, TPropType>) printingConfig).PrintingFunction((TPropType)obj);

            return printingConfig;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
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

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (typesToExclude.Contains(propertyInfo.PropertyType))
                    continue;
                if (propertiesToExclude.Contains(propertyInfo.Name))
                    continue;

                sb.Append(indentation + propertyInfo.Name + " = ");
                if (printersForProperties.ContainsKey(propertyInfo.Name))
                    sb.Append(printersForProperties[propertyInfo.Name](propertyInfo.GetValue(obj))
                              + Environment.NewLine);
                else if (printersForTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(printersForTypes[propertyInfo.PropertyType](propertyInfo.GetValue(obj))
                    + Environment.NewLine);
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}