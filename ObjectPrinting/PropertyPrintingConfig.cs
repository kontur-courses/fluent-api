using System;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo culture);
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
            return (parentConfig as IPrintingConfig<TOwner>).AddCustomSerialization(typeof(TPropType), func);;
        }
        
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.SetCultureFor<T>(CultureInfo culture)
        {
            return (parentConfig as IPrintingConfig<TOwner>).SetTypeCulture(typeof(T), culture);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => parentConfig;
    }
}