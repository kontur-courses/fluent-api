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
        private List<Type> excludedTypes = new List<Type>();
        private List<string> excluded = new List<string>();
        private Dictionary<Type, Func<object, string>> printers = new Dictionary<Type, Func<object, string>>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var config = new PropertyPrintingConfig<TOwner, TPropType>(this);
            printers[typeof(TPropType)] = ((IPropertyPrintingConfig<TOwner, TPropType>) config).Printer;
            return config;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            //TODO add excluding by name
            excluded.Add(((MemberExpression)memberSelector.Body).Member.Name);
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

        private string GetPropertyPrintingValue(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType))
                return string.Empty;

            if (excluded.Contains(propertyInfo.Name))
                return string.Empty;

            if (printers.ContainsKey(propertyInfo.PropertyType))
                return propertyInfo.Name + " = " + printers[propertyInfo.PropertyType].Invoke(obj);
            //TODO Add trimming of strings 

            //TODO Add excluding of types 

            //TODO Add alternative way to print 

            //TODO apply configurations
            return propertyInfo.Name + " = " +
                   PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
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

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + GetPropertyPrintingValue(propertyInfo, obj, nestingLevel));
            }
            return sb.ToString();
        }
    }
}