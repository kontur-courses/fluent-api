using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public TypePrintingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public TypePrintingConfig<TOwner, TPropType> Serialize<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
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
    }

    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

        internal TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            return printingConfig;
        }
        //public PrintingConfig<TOwner> WithCulture(CultureInfo culture)
        //{
        //    return printingConfig;
        //}

    }

    interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }

    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner> (this TypePrintingConfig<TOwner, int> config, CultureInfo culture)
        {
            return ((ITypePrintingConfig<TOwner>) config).PrintingConfig;
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this TypePrintingConfig<TOwner, string> config, int length)
        {
            return ((ITypePrintingConfig<TOwner>)config).PrintingConfig;
        }
    }
}