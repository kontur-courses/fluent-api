using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class TypeSerializingContextExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>(this TypeSerializingContext<TOwner, TType> context,
            CultureInfo culture)
        where TType : IFormattable
        {
            var config = ((ITypeSerializingContext<TOwner>)context).Config;
            ((IPrintingConfig<TOwner>) config).CultureInfoSettings.Add(typeof(TType), culture);
            return config;
        }
    }
}