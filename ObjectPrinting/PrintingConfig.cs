using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private TOwner value;
        
        public PrintingConfig (TOwner value)
        {
            this.value = value;
        }
        
        public PrintingConfig ()
        {
            value = default;
        }

        public string PrintToString()
        {
            return PrintToString(value);
        }
        
        public string PrintToString(int nestingLevel)
        {
            return PrintToString(value, nestingLevel);
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
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Func<TOwner, T> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }
        
        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            return this;
        }
    }

    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
        {
            return parentConfig;
        }


        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public static class PrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }

        public static PrintingConfig<TOwner> Substring<TOwner>(this PropertyPrintingConfig<TOwner, string> config,
            int start, int end)
        {
            return (config as IPropertyPrintingConfig<TOwner>).ParentConfig;
        }
    }

    public static class PrintingObjectExtensions
    {
        public static PrintingConfig<TOwner> Printing<TOwner>(this TOwner obj)
        {
            return new PrintingConfig<TOwner>(obj);
        }
    }
}