using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static TypePrintingConfig<TOwner, int> Using<TOwner>(this TypePrintingConfig<TOwner, int> config, CultureInfo culture)
        {
            return config;
        }

        public static TypePrintingConfig<TOwner, long> Using<TOwner>(this TypePrintingConfig<TOwner, long> config, CultureInfo culture)
        {
            return config;
        }

        public static TypePrintingConfig<TOwner, string> Trunc<TOwner>(this TypePrintingConfig<TOwner, string> config, int length)
        {
            return config;
        }
    }

    interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }
}
