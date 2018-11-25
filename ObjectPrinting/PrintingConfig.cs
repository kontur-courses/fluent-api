using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Compatibility;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
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

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public TypePrintingConfig<TOwner, TPropType> Serializing<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }
    }

    public interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }

    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

        internal TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationFunc)
        {
            return printingConfig;
        }
    }

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, int> config,
            CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>) config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, long> config,
            CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(
            this TypePrintingConfig<TOwner, double> config,
            CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> SubstringValue<TOwner>(
            this TypePrintingConfig<TOwner, string> config,
            int start,
            int finish)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;;
        }
    }

    public static class ObjectExtensions
    {
        public static string Serialize<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string Serialize<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}