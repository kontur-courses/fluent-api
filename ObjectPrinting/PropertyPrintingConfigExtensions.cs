using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
        {
            var cultureFunc = new Func<string, string>(s => s.Substring(0, Math.Min(length, s.Length)));
            var func = new Func<object, string>(obj => cultureFunc((string)obj));
            var prop = propertyPrintingConfig.printingConfig;
            prop.PropertySerializers.Add(propertyPrintingConfig.propertyInfo, func);

            return prop;
        }
    }
}
