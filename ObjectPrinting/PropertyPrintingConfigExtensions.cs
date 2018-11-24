using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> printingConfig,
            CultureInfo cultureInfo)
        {
            return ((IPropertyPrintingConfig<TOwner>) printingConfig).PrintingConfig;
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> printingConfig,
            int count)
        {
            return ((IPropertyPrintingConfig<TOwner>)printingConfig).PrintingConfig; // не то
        }
    }
}
