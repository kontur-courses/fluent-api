using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Defines format settings for including properties
        /// </summary>
        /// <typeparam name="T">IFormattable type</typeparam>
        /// <param name="culture">Culture for IFormattable types </param>
        /// <returns>PrintingConfig&lt;TOwner&gt;</returns>
        public static PrintingConfig<TOwner> UsingWithFormatting<TOwner, T>(this PropertyConfig<TOwner, T> config,
            CultureInfo culture, string format = null) where T : IFormattable
        {
            return config.Using(type => type.ToString(format, culture));
        }

        /// <summary>
        /// Trim string for object printer
        /// </summary>
        /// <param name="maxLen">Length maximum setting for string</param>
        /// <returns>PrintingConfig&lt;TOwner&gt;</returns>
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyConfig<TOwner, string> propertyConfig,
            int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLen));
            propertyConfig.PrintingConfig.PropertyLenForString[propertyConfig.Properties] = maxLen;
            return propertyConfig.PrintingConfig;
        }
    }
}
