using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class TypeSerializingContext<TOwner, TType> : ITypeSerializingContext<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;

        public TypeSerializingContext(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializingMethod)
        {
            return config;
        }

        PrintingConfig<TOwner> ITypeSerializingContext<TOwner>.Config => config;
    }

    public interface ITypeSerializingContext<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
    }

    public static class TypeSerializingContextExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializingContext<TOwner, int> context,
            CultureInfo culture)
        {
            return ((ITypeSerializingContext<TOwner>)context).Config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializingContext<TOwner, long> context,
            CultureInfo culture)
        {
            return ((ITypeSerializingContext<TOwner>)context).Config;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializingContext<TOwner, double> context,
            CultureInfo culture)
        {
            return ((ITypeSerializingContext<TOwner>)context).Config;
        }
    }
}