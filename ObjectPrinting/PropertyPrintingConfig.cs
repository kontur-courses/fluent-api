using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo propertyInfo)
        {
            this.property = propertyInfo;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (property is null)
                printingConfig.TypeCustomPrintings[typeof(TPropType)] = o => print((TPropType)o);
            else
                printingConfig.MemberCustomPrinting[property] =  o => print((TPropType)o);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture) 
        {
            var toStringWithCulture = typeof(TPropType).GetMethod("ToString", new[] { typeof(IFormatProvider) });
            return toStringWithCulture == null ? printingConfig : 
                Using(o => (string)toStringWithCulture.Invoke(o, new object[] { culture })) ;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
    

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}