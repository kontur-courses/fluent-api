using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, double> propertySerializingConfig, CultureInfo cultureInfo)
        {
            var printingConfig = (propertySerializingConfig as IPropertySerializingConfig<TOwner>).ParentConfig;
            ((IPrintingConfig) printingConfig).typeSerialisation.Add(typeof(double), new Func<object, string>(x =>
            {
                var num = (double)x;
                return num.ToString(cultureInfo);
            }));
            return printingConfig;
        }

        public static PrintingConfig<TOwner> TakeOnly<TOwner>(this PropertySerializingConfig<TOwner, string> config,
            int numberChar)
        {
            var printingConfig = (config as IPropertySerializingConfig<TOwner>).ParentConfig;
            (printingConfig as IPrintingConfig).typeSerialisation.Add(typeof(string), 
                new Func<object, string>(v =>
                {
                    var str = (string) v;
                    return string.IsNullOrEmpty(str) ? "" : str.Substring(0, numberChar);
                }));
            return printingConfig;
        }
    }
}