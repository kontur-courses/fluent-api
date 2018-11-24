using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ConfigsBunch configsBunch = new ConfigsBunch();

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            configsBunch.TypesToExclude.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression)
            {
                var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
                configsBunch.PropertiesToExclude.Add(propertyInfo.Name);
            }

            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            configsBunch.PrintersForTypes[typeof(TPropType)] =
                obj => (printingConfig as IPropertyPrintingConfig<TOwner, TPropType>).PrintingFunction((TPropType) obj);
            return printingConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            if (memberSelector.Body is MemberExpression)
            {
                var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
                configsBunch.PrintersForProperties[propertyInfo.Name] = 
                    obj => ((IPropertyPrintingConfig<TOwner, TPropType>) printingConfig).PrintingFunction((TPropType)obj);
            }

            return printingConfig;
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
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (configsBunch.TypesToExclude.Contains(propertyInfo.PropertyType))
                    continue;
                if (configsBunch.PropertiesToExclude.Contains(propertyInfo.Name))
                    continue;

                sb.Append(indentation + propertyInfo.Name + " = ");
                if (configsBunch.PrintersForProperties.ContainsKey(propertyInfo.Name))
                    sb.Append(configsBunch.PrintersForProperties[propertyInfo.Name](propertyInfo.GetValue(obj))
                              + Environment.NewLine);
                else if (configsBunch.PrintersForTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(configsBunch.PrintersForTypes[propertyInfo.PropertyType](propertyInfo.GetValue(obj))
                    + Environment.NewLine);
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}