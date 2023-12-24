using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypePrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, double> typeConfig, CultureInfo culture)
        {
            var culturedFunc = new Func<double, string>(d => d.ToString(culture));
            var castedFunc = new Func<object, string>(obj => culturedFunc((double)obj));
            var parent = ((IPropertyPrintingConfig<TOwner, double>)typeConfig).ParentConfig;
            parent.typeSerializers.Add(typeof(double), castedFunc);
            return parent;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, float> typeConfig, CultureInfo culture)
        {
            var culturedFunc = new Func<float, string>(f => f.ToString(culture));
            var castedFunc = new Func<object, string>(obj => culturedFunc((float)obj));
            var parent = ((IPropertyPrintingConfig<TOwner, float>)typeConfig).ParentConfig;
            parent.typeSerializers.Add(typeof(float), castedFunc);
            return parent;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypePrintingConfig<TOwner, DateTime> typeConfig, CultureInfo culture)
        {
            var culturedFunc = new Func<DateTime, string>(d => d.ToString(culture));
            var castedFunc = new Func<object, string>(obj => culturedFunc((DateTime)obj));
            var parent = ((IPropertyPrintingConfig<TOwner, DateTime>)typeConfig).ParentConfig;
            parent.typeSerializers.Add(typeof(DateTime), castedFunc);
            return parent;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this TypePrintingConfig<TOwner, string> typeConfig, int maxLen)
        {
            var culturedFunc = new Func<string, string>(s => s[..Math.Min(maxLen, s.Length)]);
            var castedFunc = new Func<object, string>(obj => culturedFunc((string)obj));
            var parent = ((IPropertyPrintingConfig<TOwner, DateTime>)typeConfig).ParentConfig;
            parent.typeSerializers.Add(typeof(string), castedFunc);
            return parent;
        }
    }
}