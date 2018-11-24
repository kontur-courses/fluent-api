using System;

namespace ObjectPrinting
{
    public class PropertySerializingContext<TOwner, TType> : IPropertySerializingContext<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;

        public PropertySerializingContext(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializingMethod)
        {
            return config;
        }

        PrintingConfig<TOwner> IPropertySerializingContext<TOwner>.Config => config;
    }

    public interface IPropertySerializingContext<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
    }

    public static class PropertySerializingContextExtensions
    {
        public static PrintingConfig<TOwner> TrimmingToLength<TOwner>(
            this PropertySerializingContext<TOwner, string> context, int length)
        {
            return ((IPropertySerializingContext<TOwner>) context).Config;
        }
    }
}