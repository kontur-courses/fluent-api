using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{

    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printConfig)
        {
            this.printConfig = printConfig;
        }

        public TypePrintingConfig(PrintingConfig<TOwner> printConfig, Func<TOwner, TPropType> func)
        {
            this.printConfig = printConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serFunc)
        {
            return printConfig;
        }
    }

    interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }
     
    public static class TypePrintingConfigExtention
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> me, CultureInfo culture)
        {
            return (me as ITypePrintingConfig<TOwner>).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Trimming<TOwner>(this TypePrintingConfig<TOwner, string> me, int len)
        {
            return (me as ITypePrintingConfig<TOwner>).PrintingConfig;
        }
    }

    public class PrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Func<TOwner, TPropType> func)
        {
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> NewSerialise<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public TypePrintingConfig<TOwner, TPropType> NewSerialise<TPropType>(Func<TOwner, TPropType> func)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this, func);
        }

        //public PropertyPrintingConfig<TOwner, string> NewSerialise(Func<TOwner, string> func, int len)
        //{
        //    return new PropertyPrintingConfig<TOwner, string>(this, (s) => (s as string).Substring(0, len));
        //}

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
}