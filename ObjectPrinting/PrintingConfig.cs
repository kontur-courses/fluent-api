using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> exludingList { get; set; }
        public string PrintToString(TOwner obj)
        {
            exludingList = new HashSet<Type>();
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
            if (exludingList.Contains(obj.GetType()))
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

        public PrintingConfig<TOwner> Exluding<T>()
        {
            exludingList.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Exluding<T>(Expression<Func<TOwner, T>> getProperty)
        {
            var propInfo = ((MemberExpression)getProperty.Body).Member as PropertyInfo;
            return this;
        }
        public PrintingConfig<TOwner> Serialize<T>(Func<T, string> f)
        {
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> func)
        {
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;

            return new PropertyPrintingConfig<TOwner, T>(this);
        }

    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
        {
            return parentConfig;
        }
    }

    public static class PringtingExtensisons
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo info)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int count)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

    }
}