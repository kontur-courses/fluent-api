using System;

namespace ObjectPrinting.PrintingConfig
{
    public static class PrintingConfigExtensions
    {
        public static PropertyPrintingConfig<TOwner, string> CutAfter<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int number)
        {
            return new PropertyPrintingConfig<TOwner, string>(number, config);
        }
        
        public static TypePrintingConfig<TOwner, string> CutAfter<TOwner>(this TypePrintingConfig<TOwner, string> config, int number)
        {
            return new TypePrintingConfig<TOwner, string>(number, config);
        }
    }
}