using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> config, CultureInfo currentCulture)
        {
            var func = new Func<int, string>(number => number + " " + currentCulture);
            return ((config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>).AddCustomPrint(func);
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> config, int length)
        {
            var func = new Func<string, string>((a) => a.Substring(0, a.Length > length ? length : a.Length));
            return ((config as IPropertyPrintingConfig<TOwner>).ParentConfig as IPrintingConfig<TOwner>).AddCustomPrint(func);
        }
    }
}
