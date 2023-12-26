using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TParent> TruncateLength<TParent>
            (this PropertyPrintingConfig<TParent, string> config, 
            int maxLength)
        {
            config.Using(o => o.Substring(0, Math.Min(maxLength, o.Length)));

            return config.Parent;
        }

        public static PrintingConfig<TParent> SetCulture<TParent>
            (this PropertyPrintingConfig<TParent, double> config,
            CultureInfo culture)
        {
            config.Using(o => o.ToString(culture));

            return config.Parent;
        }

        public static PrintingConfig<TParent> SetCulture<TParent>
            (this PropertyPrintingConfig<TParent, float> config,
            CultureInfo culture)
        {
            config.Using(o => o.ToString(culture));

            return config.Parent;
        }
    }
}