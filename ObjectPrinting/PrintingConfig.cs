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
        private readonly List<Type> excludedPropertyTypes = new List<Type>();
        private readonly List<string> excludedPropertyNames = new List<string>();
        public readonly Dictionary<string, int> propertyNamesToTrimValue = new Dictionary<string, int>();
        public readonly Dictionary<Type, CultureInfo> cultureInfo = new Dictionary<Type, CultureInfo>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression) memberSelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedPropertyNames.Add(((MemberExpression) memberSelector.Body).Member.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedPropertyTypes.Add(typeof(TPropType));
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
                if (cultureInfo.ContainsKey(obj.GetType()))
                    return string.Format(cultureInfo[obj.GetType()], "{0}", obj) + Environment.NewLine;
                else
                    return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            var propertyInfos = type.GetProperties().Where(p => !excludedPropertyTypes.Contains(p.PropertyType)
                                                                && !excludedPropertyNames.Contains(p.Name));
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(obj);
                if (propertyNamesToTrimValue.ContainsKey(propertyName))
                    propertyValue = Trim(propertyName, (string)propertyValue);

                sb.Append(identation + propertyName + " = " +
                          PrintToString(propertyValue, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string Trim(string propertyName, string propertyValue)
        {
            var requiredLength = propertyNamesToTrimValue[propertyName];
            var actualLength = propertyValue.Length;
            return propertyValue.Substring(0, Math.Min(requiredLength, actualLength));
        }
    }
}