using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public readonly Func<TOwner, TPropType> MemberSelector;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Func<TOwner, TPropType> memberSelector)
        {
            this.MemberSelector = memberSelector;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (MemberSelector is null)
            {
                printingConfig.SerializationsForType[typeof(TPropType)] = print;
            }
            else
            {
                printingConfig.SerializationForProperty[MemberSelector] = print;
            }
            return printingConfig;
        }
        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.Cultures.Add(typeof(TPropType), culture);
            return printingConfig;
        }
        
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
    
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}