using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> WithCulture<TOwner>
            (this PropertySerializationConfig<TOwner, int> config, CultureInfo culture)
        {
            return new PrintingConfig<TOwner>((config as IPropertySerializationConfig<TOwner>).ParentConfig)
                .WithDefaultTypeRule(
                    typeof(int),
                    x => ((int) x).ToString(culture));
        }

        public static PrintingConfig<TOwner> WithCulture<TOwner>
            (this PropertySerializationConfig<TOwner, double> config, CultureInfo culture)
        {
            return new PrintingConfig<TOwner>((config as IPropertySerializationConfig<TOwner>).ParentConfig)
                .WithDefaultTypeRule(
                    typeof(double),
                    x => ((double) x).ToString(culture));
        }

        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertySerializationConfig<TOwner, string> config, int length)
        {
            if(length < 0)
                throw new ArgumentOutOfRangeException();
            return new PrintingConfig<TOwner>((config as IPropertySerializationConfig<TOwner>).ParentConfig)
                .WithDefaultTypeRule(
                    typeof(string),
                    x =>
                    {
                        var str = (string)x;
                        return str.Substring(0, length > str.Length ? str.Length : length);
                    });
        }
    }
}