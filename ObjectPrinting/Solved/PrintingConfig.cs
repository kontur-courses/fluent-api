using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<Type, Delegate> printingFunctions =
            new Dictionary<Type, Delegate>();

        private readonly Dictionary<Type, CultureInfo> typeCulture =
            new Dictionary<Type, CultureInfo>();

        public void AddPrintingFunction<TPropType>(Func<TPropType, string> func)
        {
            printingFunctions[typeof(TPropType)] = func;
        }

        public void AddCultureForType<TPropType>(CultureInfo culture)
        {
            if (typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                typeCulture[typeof(TPropType)] = culture;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()))
            {
                return typeCulture.ContainsKey(obj.GetType()) 
                        ? string.Format(typeCulture[obj.GetType()], "{0}" + Environment.NewLine, obj)
                        : obj + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var resultString = new StringBuilder();
            var type = obj.GetType();
            resultString.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties().Where(prop => !excludingTypes.Contains(prop.PropertyType)))
            {
                var propertyValue = printingFunctions.ContainsKey(propertyInfo.PropertyType)
                    ? ChangeWithPrintingFunction(propertyInfo.PropertyType, propertyInfo.GetValue(obj))
                    : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                resultString.Append(indentation + propertyInfo.Name + " = " + propertyValue);
            }

            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => !excludingTypes.Contains(prop.FieldType)))
            {
                var propertyValue = printingFunctions.ContainsKey(fieldInfo.FieldType)
                    ? ChangeWithPrintingFunction(fieldInfo.FieldType, fieldInfo.GetValue(obj))
                    : PrintToString(fieldInfo.GetValue(obj), nestingLevel + 1);
                resultString.Append(indentation + fieldInfo.Name + " = " + propertyValue);
            }
            return resultString.ToString();
        }

        private string ChangeWithPrintingFunction(Type propertyType, object value)
        {
            return printingFunctions[propertyType].DynamicInvoke(value)?.ToString();
        }
    }
}