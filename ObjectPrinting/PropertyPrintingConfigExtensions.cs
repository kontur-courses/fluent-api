﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            return propConfig.Using(str => str.Substring(Math.Min(str.Length, maxLength)));
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo culture) where TPropType : IFormattable
        {
            return propConfig.Using(obj => obj.ToString(null, culture));
        }
    }
}
