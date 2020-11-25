using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public readonly string MemberName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string memberName)
        {
            this.MemberName = memberName;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (MemberName is null)
            {
                printingConfig.AddSerializationForType(typeof(TPropType), print);
            }
            else
            {
                Console.WriteLine(MemberName);
                printingConfig.AddSerializationForProperty(MemberName, print);
            }
            return printingConfig;
        }
        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.AddCultureForNumber(typeof(TPropType), culture);
            return printingConfig;
        }
        
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
    
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}