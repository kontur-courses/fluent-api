using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
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

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            return this;
        }


//        public PrintingConfig<TOwner> SetCulture(CultureInfo currentCulture)
//        {
//            return this;
//        }

        public PrintingConfig<TOwner,TPropType> Serializing<TPropType>()
        {
            return new PrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner,TPropType> Serializing<TPropType>(Expression<Func<TOwner,TPropType>> selector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this,selector);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner,TPropType>> selector)
        {
            return this;
        }
    }
    
    public class PrintingConfig<TOwner,TPropType> : IPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public PrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }
    
    
    public class PropertyPrintingConfig<TOwner,TPropType> : IPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> selector;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner,TPropType>>  selector)
        {
            this.printingConfig = printingConfig;
            this.selector = selector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            return printingConfig;
        }
        PrintingConfig<TOwner> IPrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }

    public interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }

    public static class PrintingConfigExtenstion
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PrintingConfig<TOwner, int> printingConfig,
            CultureInfo currentCulture)
        {
            return ((IPrintingConfig<TOwner>)printingConfig).PrintingConfig;
        }
        
        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> printingConfig,
            int length)
        {
            return ((IPrintingConfig<TOwner>)printingConfig).PrintingConfig;
        }
    }
}