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
        private HashSet<Type> excludedTypes;

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
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> ForProperty<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> ForProperty<T>(Expression<Func<TOwner, T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression;
            return new PropertyPrintingConfig<TOwner, T>(this, memberExpr.Member);
        }
    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
        private readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
            memberInfo = null;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo memberInfo)
        {
            this.parentConfig = parentConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> SetFormat(Func<TPropType, string> serializationFunc)
        {
            return parentConfig;
        }
    }

    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> SetFormat<TOwner>(
            this PropertyPrintingConfig<TOwner, int> config, CultureInfo cultureInfo)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Cut<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config, int amount)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static string ToFormatString<TOwner>(
            this TOwner formattedObject)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(formattedObject);
        }
    }
}