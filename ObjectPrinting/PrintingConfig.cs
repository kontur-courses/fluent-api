using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ConfigInfo configInfo = new ConfigInfo();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            configInfo.Exclude(memberSelector.GetPropertyInfo());
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            configInfo.Exclude(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(
            Func<TPropType, string> print)
        {
            configInfo.RegisterCustomPrinter(
                typeof(TPropType),
                obj => print((TPropType) obj));
            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector,
            Func<TPropType, string> print)
        {
            configInfo.RegisterCustomPrinter(
                memberSelector.GetPropertyInfo(),
                obj => print((TPropType) obj));
            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(CultureInfo culture)
            where TPropType : IFormattable
        {
            configInfo.RegisterCulture(typeof(TPropType), culture);
            return this;
        }

        public PrintingConfig<TOwner> TrimmedToLength(
            Expression<Func<TOwner, string>> memberSelector, int maxLength)
        {
            configInfo.SetMaxLength(
                memberSelector.GetPropertyInfo(),
                maxLength);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToStringFinalType(object obj)
        {
            if (obj is IFormattable && configInfo.TryGetCulture(obj.GetType(), out var culture))
                return ((IFormattable) obj).ToString(null, culture) + Environment.NewLine;
            return obj + Environment.NewLine;
        }

        private string PrintToStringProperty(PropertyInfo propInfo, object obj, int nestingLevel)
        {
            var strProperty = "";
            var identation = new string('\t', nestingLevel + 1);
            if (configInfo.TryGetCustomPrinter(propInfo, out var printer))
                strProperty = printer(propInfo.GetValue(obj));
            else if (configInfo.TryGetCustomPrinter(propInfo.PropertyType, out printer))
                strProperty = printer(propInfo.GetValue(obj));
            else
                strProperty = PrintToString(propInfo.GetValue(obj), nestingLevel + 1);
            if (propInfo.PropertyType == typeof(string)
                && configInfo.TryGetMaxLength(propInfo, out var maxLength))
                strProperty = strProperty.Substring(0, Math.Min(maxLength, strProperty.Length));
            return identation + propInfo.Name + " = " + strProperty;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (configInfo.IsFinal(type))
                return PrintToStringFinalType(obj);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (configInfo.IsExcluded(propertyInfo.PropertyType)
                    || configInfo.IsExcluded(propertyInfo))
                    continue;
                sb.Append(PrintToStringProperty(propertyInfo, obj, nestingLevel));
            }

            return sb.ToString();
        }
    }
}