using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static PrintingConfig<TOwner> Serialized<TOwner>(this TOwner obj)
        {
            return new PrintingConfig<TOwner>();
        }
    }
}